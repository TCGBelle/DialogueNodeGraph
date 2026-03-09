using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NarativeNodeGraph.Models;
using System.Collections.ObjectModel;
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

    public ICommand MouseMoveOnCanvasCommand { get; }
    public IRelayCommand CanvasMouseUpCommand { get; }
    public IRelayCommand<ConnectionViewModel> DeleteConnectionCommand { get; }
    public IRelayCommand<NodeKind> AddNodeCommand { get; }
    public IRelayCommand<Point> CanvasRightClickCommand { get; }

    public GraphViewModel()
    {

        var startNode = CreateNodeOfType(NodeKind.Start, 100, 100);
        var endNode = CreateNodeOfType(NodeKind.End, 400, 200);

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
        });
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
        System.Diagnostics.Debug.WriteLine("StartConnection fired");
        CancelPreview();
        Mouse.OverrideCursor = Cursors.Cross;
        activePort = port;

        previewConnection = new ConnectionViewModel(port, null, DeleteConnectionCommand)
        {
            OverrideEndPoint = GetPortPoint(port) // start at the port
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
    }

    private bool CanConnect(PortViewModel from, PortViewModel to)
    {
        if (from.Type != PortType.Output || to.Type != PortType.Input)
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
            ? node.NodeWidth
            : 0;

        return new Point(node.X + offsetX, node.Y + offsetY);
    }

    public void SetContextMenuPosition(Point position)
    {
        lastContextMenuPosition = position;
    }
}

