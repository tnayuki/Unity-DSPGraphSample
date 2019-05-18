using Unity.Collections;
using Unity.Experimental.Audio;
using Unity.Mathematics;
using UnityEngine;

using MidiJack;

public class Demo3 : MonoBehaviour
{
    private DSPNode[] modulatorNode = new DSPNode[128];
    private DSPNode[] carrierNode = new DSPNode[128];

    void Start()
    {
        MidiMaster.noteOnDelegate += (MidiChannel channel, int noteNumber, float velocity) => {
            var graph = DSPGraph.GetDefaultGraph();
            var block = graph.CreateCommandBlock();

            modulatorNode[noteNumber] = block.CreateDSPNode<SineOscillatorNodeJob.Params, NoProvs, SineOscillatorNodeJob>();
            block.AddOutletPort(modulatorNode[noteNumber], 2, SoundFormat.Stereo);
            block.SetFloat<SineOscillatorNodeJob.Params, NoProvs, SineOscillatorNodeJob>(
                modulatorNode[noteNumber], 
                SineOscillatorNodeJob.Params.Frequency, 2.0f * math.pow(2, (noteNumber - 69) / 12.0f) * 440.0f
            );

            carrierNode[noteNumber] = block.CreateDSPNode<ModulatableSineOscillatorNodeJob.Params, NoProvs, ModulatableSineOscillatorNodeJob>();
            block.AddInletPort(carrierNode[noteNumber], 2, SoundFormat.Stereo);
            block.AddOutletPort(carrierNode[noteNumber], 2, SoundFormat.Stereo);
            block.SetFloat<ModulatableSineOscillatorNodeJob.Params, NoProvs, ModulatableSineOscillatorNodeJob>(
                carrierNode[noteNumber], 
                ModulatableSineOscillatorNodeJob.Params.Frequency, math.pow(2, (noteNumber - 69) / 12.0f) * 440.0f
            );

            block.Connect(modulatorNode[noteNumber], 0, carrierNode[noteNumber], 0);
            block.Connect(carrierNode[noteNumber], 0, graph.GetRootDSP(), 0);

            block.Complete();
        };

        MidiMaster.noteOffDelegate += (MidiChannel channel, int noteNumber) => {
            var graph = DSPGraph.GetDefaultGraph();
            var block = graph.CreateCommandBlock();

            block.ReleaseDSPNode(modulatorNode[noteNumber]);
            block.ReleaseDSPNode(carrierNode[noteNumber]);

            block.Complete();
        };
    }
}
