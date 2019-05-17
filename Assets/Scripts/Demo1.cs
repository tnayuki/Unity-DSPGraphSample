using Unity.Experimental.Audio;
using UnityEngine;

public class Demo1 : MonoBehaviour
{
    void Start()
    {
        var graph = DSPGraph.GetDefaultGraph();

        var block = graph.CreateCommandBlock();
        var node = block.CreateDSPNode<SineOscillatorNodeJob.Params, NoProvs, SineOscillatorNodeJob>();
        block.AddOutletPort(node, 2, SoundFormat.Stereo);
        block.SetFloat<SineOscillatorNodeJob.Params, NoProvs, SineOscillatorNodeJob>(node, SineOscillatorNodeJob.Params.Frequency, 440.0f);

        var connection = block.Connect(node, 0, graph.GetRootDSP(), 0);

        block.Complete();
    }
}
