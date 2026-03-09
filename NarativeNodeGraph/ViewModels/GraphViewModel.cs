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

    private PortViewModel? _activePort;
    private ConnectionViewModel? _previewConnection;
    public ICommand MouseMoveOnCanvasCommand { get; }
    public IRelayCommand CanvasMouseUpCommand { get; }
    public IRelayCommand<ConnectionViewModel> DeleteConnectionCommand { get; }

    public GraphViewModel()
    {

        var startNode = CreateNodeOfType(NodeKind.Start, 100, 100);
        var endNode = CreateNodeOfType(NodeKind.End, 400, 200);

        Nodes.Add(startNode);
        Nodes.Add(endNode);
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

    public bool IsConnecting => _activePort != null;

    /// <summary>
    /// Called when user presses mouse on a port.
    /// Creates a preview connection (From = port, To = null).
    /// </summary>
    public void StartConnection(PortViewModel port)
    {
        System.Diagnostics.Debug.WriteLine("StartConnection fired");
        CancelPreview();
        Mouse.OverrideCursor = Cursors.Cross;
        _activePort = port;

        _previewConnection = new ConnectionViewModel(port, null, DeleteConnectionCommand)
        {
            OverrideEndPoint = GetPortPoint(port) // start at the port
        };

        Connections.Add(_previewConnection);
        OnPropertyChanged(nameof(IsConnecting));
    }

    /// <summary>
    /// Called as mouse moves while connecting. Updates the preview endpoint.
    /// </summary>
    public void UpdatePreviewEndPoint(Point mouseCanvasPoint)
    {
        System.Diagnostics.Debug.WriteLine("UpdatePreviewEndPoint: " + mouseCanvasPoint);
        if (_previewConnection == null)
            return;

        _previewConnection.OverrideEndPoint = mouseCanvasPoint;
        _previewConnection.RaiseGeometryChanged();
    }

    /// <summary>
    /// Called when user releases mouse on a port.
    /// </summary>
    public void CompleteConnection(PortViewModel targetPort)
    {
        if (_activePort == null)
            return;

        // Remove preview line first
        CancelPreview();

        // Don’t connect to itself
        if (ReferenceEquals(_activePort, targetPort))
        {
            _activePort = null;
            OnPropertyChanged(nameof(IsConnecting));
            return;
        }

        // Must be Input<->Output
        if (_activePort.Type == targetPort.Type)
        {
            _activePort = null;
            OnPropertyChanged(nameof(IsConnecting));
            return;
        }

        // Normalize direction: From = Output, To = Input
        var from = _activePort.Type == PortType.Output ? _activePort : targetPort;
        var to = _activePort.Type == PortType.Output ? targetPort : _activePort;

        Connections.Add(new ConnectionViewModel(from, to, DeleteConnectionCommand));

        _activePort = null;
        OnPropertyChanged(nameof(IsConnecting));
    }

    /// <summary>
    /// Called when user releases on empty space / cancels.
    /// </summary>
    public void CancelConnection()
    {
        CancelPreview();
        _activePort = null;
        OnPropertyChanged(nameof(IsConnecting));
    }

    private void CancelPreview()
    {
        if (_previewConnection != null)
            Connections.Remove(_previewConnection);
        Mouse.OverrideCursor = null;
        _previewConnection = null;
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
}

