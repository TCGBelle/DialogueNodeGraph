using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NarativeNodeGraph.Models;
using NarativeNodeGraph.Services;
using NarativeNodeGraph.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NarativeNodeGraph.ViewModels;

public partial class GraphViewModel : ObservableObject
{
    public ObservableCollection<NodeViewModel> Nodes { get; } = new();
    public ObservableCollection<ConnectionViewModel> Connections { get; } = new();

    private PortViewModel? activePort;
    private ConnectionViewModel? previewConnection;
    private Point lastContextMenuPosition;
    private readonly GraphSerializationMapper _mapper = new();
    private readonly GraphJsonFileService _fileService = new();
    private readonly IFileDialogService fileDialogService;
    [ObservableProperty]
    private bool isDirty = false;
    [ObservableProperty]
    private string? currentFilePath;
    public string DisplayTitle
    {
        get
        {
            var fileName = string.IsNullOrWhiteSpace(CurrentFilePath)
                ? "Untitled"
                : System.IO.Path.GetFileNameWithoutExtension(CurrentFilePath);

            return IsDirty ? $"{fileName} *" : fileName;
        }
    }

    [ObservableProperty]
    private double zoom = 1.0;

    [ObservableProperty]
    private double panX = -50000 + 200;

    [ObservableProperty]
    private double panY = -50000 + 200;
    public ICommand MouseMoveOnCanvasCommand { get; }
    public IRelayCommand CanvasMouseUpCommand { get; }
    public IRelayCommand<ConnectionViewModel> DeleteConnectionCommand { get; }
    public IRelayCommand<NodeKind> AddNodeCommand { get; }
    public IRelayCommand<NodeViewModel> DeleteNodeCommand { get; }
    public IRelayCommand<Point> CanvasRightClickCommand { get; }
    public IRelayCommand SaveCommand { get; }
    public IRelayCommand LoadCommand { get; }

    public GraphViewModel(IFileDialogService _fileDialogService)
    {
        fileDialogService = _fileDialogService ?? throw new ArgumentNullException(nameof(_fileDialogService));
        var startNode = CreateNodeOfType(NodeKind.Start, 50000, 50000);
        var endNode = CreateNodeOfType(NodeKind.End, 50300, 50100);

        Nodes.Add(startNode);
        Nodes.Add(endNode);
        CanvasRightClickCommand = new RelayCommand<Point>(p =>
        {
            System.Diagnostics.Debug.WriteLine($"Stored menu position: {p}");
            lastContextMenuPosition = p;
        });
        AddNodeCommand = new RelayCommand<NodeKind>(kind =>
        {
            var node = CreateNodeOfType(kind, lastContextMenuPosition.X, lastContextMenuPosition.Y);
            Nodes.Add(node);
        });
        MouseMoveOnCanvasCommand = new RelayCommand<Point>(p =>
        {
            System.Diagnostics.Debug.WriteLine("Mouse move: " + p);
            if (IsConnecting)
                UpdatePreviewEndPoint(p);
        });

        CanvasMouseUpCommand = new RelayCommand(() =>
        {
            if (IsConnecting)
                CancelConnection();
        });
        DeleteConnectionCommand = new RelayCommand<ConnectionViewModel>(c =>
        {
            if (c != null)
                Connections.Remove(c);
            IsDirty = true;
            OnPropertyChanged(nameof(DisplayTitle));
        });
        DeleteNodeCommand = new RelayCommand<NodeViewModel>(DeleteNode);

        SaveCommand = new AsyncRelayCommand(SaveAsync);
        LoadCommand = new AsyncRelayCommand(LoadAsync);
    }

    private NodeViewModel CreateNodeOfType(NodeKind kind, double x, double y)
    {
        var model = new NodeModel
        {
            Id = Guid.NewGuid(),
            X = x,
            Y = y,
            Title = kind switch
            {
                NodeKind.Start => "Start",
                NodeKind.End => "End",
                NodeKind.NpcDialogue => "NPC Dialogue",
                NodeKind.Answer => "Answer",
                NodeKind.PlayerDialogue => "Player Dialogue",
                _ => "Node"
            }
        };
        IsDirty = true;
        OnPropertyChanged(nameof(DisplayTitle));
        return kind switch
        {
            NodeKind.Start => new StartNodeViewModel(model, this),
            NodeKind.End => new EndNodeViewModel(model, this),
            NodeKind.NpcDialogue => new NpcDialogueNodeViewModel(model, this),
            NodeKind.Answer => new AnswerNodeViewModel(model, this),
            NodeKind.PlayerDialogue => new PlayerDialogueNodeViewModel(model, this),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
        
    }

    public bool IsConnecting => activePort != null;

    /// <summary>
    /// Called when user presses mouse on a port.
    /// Creates a preview connection (From = port, To = null).
    /// </summary>
    public void StartConnection(PortViewModel port)
    {

        if (port.Type == PortType.Output && Connections.Any(c => c.From == port))
            return; // output port already has a connection

        System.Diagnostics.Debug.WriteLine("StartConnection fired");
        CancelPreview();
        Mouse.OverrideCursor = Cursors.Cross;
        activePort = port;

        previewConnection = new ConnectionViewModel(port, null, DeleteConnectionCommand)
        {
            OverrideEndPoint = null
        };

        Connections.Add(previewConnection);
        OnPropertyChanged(nameof(IsConnecting));
    }

    /// <summary>
    /// Called as mouse moves while connecting. Updates the preview endpoint.
    /// </summary>
    public void UpdatePreviewEndPoint(Point mouseCanvasPoint)
    {
        System.Diagnostics.Debug.WriteLine("UpdatePreviewEndPoint: " + mouseCanvasPoint);
        if (previewConnection == null)
            return;

        previewConnection.OverrideEndPoint = mouseCanvasPoint;
        previewConnection.RaiseGeometryChanged();
    }

    /// <summary>
    /// Called when user releases mouse on a port.
    /// </summary>
    public void CompleteConnection(PortViewModel targetPort)
    {
        if (activePort == null)
            return;

        // Remove preview line first
        CancelPreview();

        // Don’t connect to itself
        if (ReferenceEquals(activePort, targetPort))
        {
            activePort = null;
            OnPropertyChanged(nameof(IsConnecting));
            return;
        }

        // Must be Input<->Output
        if (activePort.Type == targetPort.Type)
        {
            activePort = null;
            OnPropertyChanged(nameof(IsConnecting));
            return;
        }

        // Normalize direction: From = Output, To = Input
        var from = activePort.Type == PortType.Output ? activePort : targetPort;
        var to = activePort.Type == PortType.Output ? targetPort : activePort;

        if (!CanConnect(from, to))
        {
            activePort = null;
            OnPropertyChanged(nameof(IsConnecting));
            return;
        }

        Connections.Add(new ConnectionViewModel(from, to, DeleteConnectionCommand));

        activePort = null;
        OnPropertyChanged(nameof(IsConnecting));
        IsDirty = true;
        OnPropertyChanged(nameof(DisplayTitle));
    }

    private bool CanConnect(PortViewModel from, PortViewModel to)
    {
        if (from.Type != PortType.Output || to.Type != PortType.Input)
            return false;

        // Output ports can only have one outgoing connection
        if (Connections.Any(c => c.From == from))
            return false;

        var fromKind = from.ParentNode.Kind;
        var toKind = to.ParentNode.Kind;

        // Any node can connect to End
        if (toKind == NodeKind.End)
            return true;

        return fromKind switch
        {
            NodeKind.Start => toKind == NodeKind.NpcDialogue,
            NodeKind.NpcDialogue => toKind == NodeKind.Answer,
            NodeKind.Answer => toKind == NodeKind.PlayerDialogue,
            NodeKind.PlayerDialogue => toKind == NodeKind.NpcDialogue,
            _ => false
        };
    }

    /// <summary>
    /// Called when user releases on empty space / cancels.
    /// </summary>
    public void CancelConnection()
    {
        CancelPreview();
        activePort = null;
        OnPropertyChanged(nameof(IsConnecting));
    }

    private void CancelPreview()
    {
        if (previewConnection != null)
            Connections.Remove(previewConnection);
        Mouse.OverrideCursor = null;
        previewConnection = null;
    }

    public Point GetPortPoint(PortViewModel port)
    {
        var node = port.ParentNode;
        int indexSameSide = node.Ports.Count(p => p.Type == port.Type && !ReferenceEquals(p, port));

        double offsetY = 30 + (indexSameSide * 18) + 6;

        double offsetX = port.Type == PortType.Output
            ? node.Width
            : 0;

        return new Point(node.X + offsetX, node.Y + offsetY);
    }

    public void SetContextMenuPosition(Point position)
    {
        lastContextMenuPosition = position;
    }

    public async Task SaveToFile(string path)
    {
        var dto = _mapper.ToDto(this);
        await _fileService.SaveAsync(path, dto);
    }

    public async Task LoadFromFile(string path)
    {
        var dto = await _fileService.LoadAsync(path);

        ClearGraph();

        var result = _mapper.FromDto(dto, this);

        foreach (var node in result.Nodes)
            Nodes.Add(node);

        foreach (var connection in result.Connections)
            Connections.Add(connection);
    }

    private async Task SaveAsync()
    {
        var path = fileDialogService.ShowSaveFileDialog(
            "JSON Graph (*.json)|*.json|All Files (*.*)|*.*",
            ".json",
            "graph.json");

        if (string.IsNullOrWhiteSpace(path))
            return;

        var dto = _mapper.ToDto(this);
        await _fileService.SaveAsync(path, dto);
        CurrentFilePath = path;
        IsDirty = false;
        OnPropertyChanged(nameof(DisplayTitle));
    }

    private async Task LoadAsync()
    {
        var path = fileDialogService.ShowOpenFileDialog(
            "JSON Graph (*.json)|*.json|All Files (*.*)|*.*",
            ".json");

        if (string.IsNullOrWhiteSpace(path))
            return;

        var dto = await _fileService.LoadAsync(path);

        ClearGraph();

        var result = _mapper.FromDto(dto, this);

        foreach (var node in result.Nodes)
            Nodes.Add(node);

        foreach (var connection in result.Connections)
            Connections.Add(connection);
        CurrentFilePath = path;
        IsDirty = false;
        OnPropertyChanged(nameof(DisplayTitle));
    }

    private void ClearGraph()
    {
        CancelConnection();
        Connections.Clear();
        Nodes.Clear();
    }

    private void DeleteNode(NodeViewModel? node)
    {
        if (node is null)
            return;

        CancelConnection();

        var connectionsToRemove = Connections
            .Where(c => c.From.ParentNode == node || c.To?.ParentNode == node)
            .ToList();

        foreach (var connection in connectionsToRemove)
            Connections.Remove(connection);

        Nodes.Remove(node);
        IsDirty = true;
        OnPropertyChanged(nameof(DisplayTitle));
    }

    partial void OnIsDirtyChanged(bool value)
    {
        OnPropertyChanged(nameof(DisplayTitle));
    }

    partial void OnCurrentFilePathChanged(string? value)
    {
        OnPropertyChanged(nameof(DisplayTitle));
    }

    public void DeletePort(PortViewModel port)
    {
        if (port.Type == PortType.Output && port.ParentNode.OutputPorts.Count() <= 1)
            return; // keep at least one output

        var connectionsToRemove = Connections
            .Where(c => c.From == port || c.To == port)
            .ToList();

        foreach (var connection in connectionsToRemove)
            Connections.Remove(connection);

        port.ParentNode.RemovePortPublic(port);

        IsDirty = true;
        OnPropertyChanged(nameof(DisplayTitle));
    }

    public GraphValidationResult ValidateGraph()
    {
        var result = new GraphValidationResult();

        var startNode = Nodes.FirstOrDefault(n => n.Kind == NodeKind.Start);
        if (startNode == null)
        {
            result.AddError("No Start node found.");
            return result;
        }

        var visited = new HashSet<NodeViewModel>();
        TraversePath(startNode, new HashSet<NodeViewModel>(), result, visited);

        // Warnings: floating output ports
        foreach (var node in Nodes)
        {
            foreach (var port in node.OutputPorts)
            {
                bool hasConnection = Connections.Any(c => c.From == port);
                if (!hasConnection)
                    result.AddWarning($"'{node.Title}' has an unconnected output port ('{port.Label}').");
            }
        }

        // Warnings: orphaned nodes (unreachable from Start)
        foreach (var node in Nodes)
        {
            if (node != startNode && !visited.Contains(node))
                result.AddWarning($"'{node.Title}' is not reachable from Start.");
        }

        // Warnings: empty dialogue/answer text
        foreach (var node in Nodes)
        {
            string? text = node switch
            {
                NpcDialogueNodeViewModel npc => npc.DialogueText,
                PlayerDialogueNodeViewModel player => player.DialogueText,
                AnswerNodeViewModel answer => answer.AnswerText,
                _ => null
            };

            if (text != null && string.IsNullOrWhiteSpace(text))
                result.AddWarning($"'{node.Title}' has empty text.");
        }

        return result;
    }

    private void TraversePath(
        NodeViewModel node,
        HashSet<NodeViewModel> pathVisited,
        GraphValidationResult result,
        HashSet<NodeViewModel> everVisited)
    {
        everVisited.Add(node);

        if (node.Kind == NodeKind.End)
            return; // reached an End node — this path is valid

        if (pathVisited.Contains(node))
            return; // cycle detected on this path — stop recursing, don't false-flag as dead end

        pathVisited.Add(node);

        var outgoing = Connections.Where(c => c.From.ParentNode == node).ToList();

        if (outgoing.Count == 0)
        {
            result.AddError($"'{node.Title}' has no path to an End node.");
            return;
        }

        foreach (var connection in outgoing)
        {
            if (connection.To != null)
                TraversePath(connection.To.ParentNode, new HashSet<NodeViewModel>(pathVisited), result, everVisited);
        }
    }

    [RelayCommand]
    private void Validate()
    {
        var result = ValidateGraph();

        if (result.Issues.Count == 0)
        {
            MessageBox.Show("Graph is valid — no issues found.", "Validation", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var message = string.Join("\n", result.Issues.Select(i =>
            (i.IsError ? "[ERROR] " : "[WARNING] ") + i.Message));

        var icon = result.HasErrors ? MessageBoxImage.Error : MessageBoxImage.Warning;

        MessageBox.Show(message, "Validation Results", MessageBoxButton.OK, icon);
    }

    [RelayCommand]
    private void Play()
    {
        var playerViewModel = new DialoguePlayerViewModel(this);
        var window = new DialoguePlayerWindow(playerViewModel)
        {
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();
    }
}

