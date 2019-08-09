using Unity.Audio;
using UnityEngine;

public class Demo1 : MonoBehaviour
{
    private AudioOutputHandle _outputHandle;

    void Start()
    {
        var graph = DSPGraph.Create(SoundFormat.Stereo, 2, 1024, 48000);

        var driver = new DefaultDSPGraphDriver { Graph = graph };
        driver.Initialize(2, SoundFormat.Stereo, 48000, 1024);

        _outputHandle = driver.AttachToDefaultOutput();

        var block = graph.CreateCommandBlock();
        var node = block.CreateDSPNode<SineOscillatorNodeJob.Params, NoProvs, SineOscillatorNodeJob>();
        block.AddOutletPort(node, 2, SoundFormat.Stereo);
        block.SetFloat<SineOscillatorNodeJob.Params, NoProvs, SineOscillatorNodeJob>(
            node, SineOscillatorNodeJob.Params.Frequency, 440.0f);

        var connection = block.Connect(node, 0, graph.RootDSP, 0);

        block.Complete();
    }

    void OnDestroy() {
        _outputHandle.Dispose();
    }
}
