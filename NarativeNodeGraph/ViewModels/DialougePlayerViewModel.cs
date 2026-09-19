using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace NarativeNodeGraph.ViewModels
{
    public partial class DialoguePlayerViewModel : ObservableObject
    {
        private readonly GraphViewModel _graph;

        [ObservableProperty]
        private string currentSpeakerText = "";

        [ObservableProperty]
        private bool isShowingChoices;

        [ObservableProperty]
        private bool isFinished;

        public ObservableCollection<AnswerNodeViewModel> CurrentChoices { get; } = new();


        public DialoguePlayerViewModel(GraphViewModel graph)
        {
            _graph = graph;
            var startNode = graph.Nodes.FirstOrDefault(n => n.Kind == NodeKind.Start);
            if (startNode != null)
                _ = Advance(startNode);
        }

        private async Task Advance(NodeViewModel node)
        {
            try
            {
                switch (node)
                {
                    case NpcDialogueNodeViewModel npc:
                        CurrentSpeakerText = npc.DialogueText;
                        IsShowingChoices = true;
                        LoadChoices(npc);
                        if (CurrentChoices.Count == 0)
                        {
                            await Task.Delay(1500);
                            await AdvanceToNext(npc);
                        }
                        break;

                    case PlayerDialogueNodeViewModel player:
                        CurrentSpeakerText = player.DialogueText;
                        IsShowingChoices = false;
                        await Task.Delay(1500);
                        await AdvanceToNext(player);
                        break;

                    case var end when end.Kind == NodeKind.End:
                        CurrentSpeakerText = "";
                        IsFinished = true;
                        break;

                    default:
                        await AdvanceToNext(node);
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error advancing dialogue: {ex.Message}");
                IsFinished = true;
            }
        }

        private void LoadChoices(NodeViewModel npcNode)
        {
            CurrentChoices.Clear();

            var orderedAnswers = _graph.Connections
                .Where(c => c.From.ParentNode == npcNode && c.To != null)
                .OrderBy(c => npcNode.OutputPorts.ToList().IndexOf(c.From))
                .Select(c => c.To!.ParentNode)
                .OfType<AnswerNodeViewModel>();

            foreach (var answer in orderedAnswers)
                CurrentChoices.Add(answer);
        }

        [RelayCommand]
        private async Task ChooseAnswer(AnswerNodeViewModel answer)
        {
            IsShowingChoices = false;
            CurrentSpeakerText = answer.AnswerText;
            await Task.Delay(500);
            await AdvanceToNext(answer);
        }

        private async Task AdvanceToNext(NodeViewModel node)
        {
            var connection = _graph.Connections.FirstOrDefault(c => c.From.ParentNode == node);

            if (connection?.To == null)
            {
                IsFinished = true;
                return;
            }

            await Advance(connection.To.ParentNode);
        }
    }
}
