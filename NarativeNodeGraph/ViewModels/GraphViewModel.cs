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
        // Sample graph
        var node1 = new NodeViewModel(new NodeModel { X = 100, Y = 100, Title = "Node 1" }, this);
        var node2 = new NodeViewModel(new NodeModel { X = 400, Y = 200, Title = "Node 2" }, this);

        var port1Out = new PortViewModel(node1, new PortModel { Name = "Out", Type = PortType.Output });
        var port2In = new PortViewModel(node2, new PortModel { Name = "In", Type = PortType.Input });

        node1.Ports.Add(port1Out);
        node2.Ports.Add(port2In);

        Nodes.Add(node1);
        Nodes.Add(node2);

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

    public bool IsConnecting => _activePort != null;

    /// <summary>
    /// Called when user presses mouse on a port.
    /// Creates a preview connection (From = port, To = null).
    /// </summary>
    public void StartConnection(PortViewModel port)
    {
        System.Diagnostics.Debug.WriteLine("StartConnection fired");
        CancelPreview();

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

        _previewConnection = null;
    }

    // Keep this in ONE place so you don’t duplicate geometry math.
    // This should match your NodeView layout (width/port spacing).
    public Point GetPortPoint(PortViewModel port)
    {
        var node = port.ParentNode;

        // If you layout ports in two columns (input left, output right),
        // the vertical index should be among same-side ports.
        int indexSameSide = node.Ports.Count(p => p.Type == port.Type && !ReferenceEquals(p, port));
        // indexSameSide above is "count before this one" (stable even if duplicates)
        // If you prefer:
        // var same = node.Ports.Where(p => p.Type == port.Type).ToList();
        // int indexSameSide = same.IndexOf(port);

        double offsetY = 30 + (indexSameSide * 18) + 6;

        double offsetX = port.Type == PortType.Output
            ? node.NodeWidth   // see note below
            : 0;

        return new Point(node.X + offsetX, node.Y + offsetY);
    }
}

