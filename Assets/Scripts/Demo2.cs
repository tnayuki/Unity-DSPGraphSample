using Unity.Audio;
using Unity.Mathematics;
using UnityEngine;

using MidiJack;

public class Demo2 : MonoBehaviour
{
    private AudioOutputHandle _outputHandle;

    private DSPNode[] notesNode = new DSPNode[128];

    void Start()
    {
        var graph = DSPGraph.Create(SoundFormat.Stereo, 2, 1024, 48000);

        var driver = new DefaultDSPGraphDriver { Graph = graph };

        _outputHandle = driver.AttachToDefaultOutput();

        MidiMaster.noteOnDelegate += (MidiChannel channel, int noteNumber, float velocity) => {
            var block = graph.CreateCommandBlock();

            notesNode[noteNumber] = block.CreateDSPNode<SineOscillatorNodeJob.Params, NoProvs, SineOscillatorNodeJob>();
            block.AddOutletPort(notesNode[noteNumber], 2, SoundFormat.Stereo);
            block.SetFloat<SineOscillatorNodeJob.Params, NoProvs, SineOscillatorNodeJob>(
                notesNode[noteNumber], 
                SineOscillatorNodeJob.Params.Frequency, math.pow(2, (noteNumber - 69) / 12.0f) * 440.0f
            );

            block.Connect(notesNode[noteNumber], 0, graph.RootDSP, 0);

            block.Complete();
        };

        MidiMaster.noteOffDelegate += (MidiChannel channel, int noteNumber) => {
            var block = graph.CreateCommandBlock();

            block.ReleaseDSPNode(notesNode[noteNumber]);

            block.Complete();
        };
    }

    void OnDestroy() {
        _outputHandle.Dispose();
    }
}
