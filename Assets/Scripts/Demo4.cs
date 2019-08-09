using Unity.Audio;
using Unity.Mathematics;
using UnityEngine;

using MidiJack;

public class Demo4 : MonoBehaviour
{
    private AudioOutputHandle _outputHandle;

    private DSPNode[] modulatorNode = new DSPNode[128];
    private DSPNode[] modulatorADSNode = new DSPNode[128];
    private DSPNode[] carrierNode = new DSPNode[128];
    private DSPNode[] carrierADSNode = new DSPNode[128];

    void Start()
    {
        var graph = DSPGraph.Create(SoundFormat.Stereo, 2, 1024, 48000);

        var driver = new DefaultDSPGraphDriver { Graph = graph };

        _outputHandle = driver.AttachToDefaultOutput();

        MidiMaster.noteOnDelegate += (MidiChannel channel, int noteNumber, float velocity) => {
            var block = graph.CreateCommandBlock();

            modulatorNode[noteNumber] = block.CreateDSPNode<SineOscillatorNodeJob.Params, NoProvs, SineOscillatorNodeJob>();
            block.AddOutletPort(modulatorNode[noteNumber], 2, SoundFormat.Stereo);
            block.SetFloat<SineOscillatorNodeJob.Params, NoProvs, SineOscillatorNodeJob>(
                modulatorNode[noteNumber], 
                SineOscillatorNodeJob.Params.Frequency, 1.34f * math.pow(2, (noteNumber - 69) / 12.0f) * 440.0f
            );

            modulatorADSNode[noteNumber] = block.CreateDSPNode<AttackDecaySustainNodeJob.Params, NoProvs, AttackDecaySustainNodeJob>();
            block.AddInletPort(modulatorADSNode[noteNumber], 2, SoundFormat.Stereo);
            block.AddOutletPort(modulatorADSNode[noteNumber], 2, SoundFormat.Stereo);
            block.SetFloat<AttackDecaySustainNodeJob.Params, NoProvs, AttackDecaySustainNodeJob>(
                modulatorADSNode[noteNumber], 
                AttackDecaySustainNodeJob.Params.Attack, 0.3f
            );

            block.SetFloat<AttackDecaySustainNodeJob.Params, NoProvs, AttackDecaySustainNodeJob>(
                modulatorADSNode[noteNumber], 
                AttackDecaySustainNodeJob.Params.Decay, 0.1f
            );

            block.SetFloat<AttackDecaySustainNodeJob.Params, NoProvs, AttackDecaySustainNodeJob>(
                modulatorADSNode[noteNumber], 
                AttackDecaySustainNodeJob.Params.Sustain, 1.2f
            );

            carrierNode[noteNumber] = block.CreateDSPNode<ModulatableSineOscillatorNodeJob.Params, NoProvs, ModulatableSineOscillatorNodeJob>();
            block.AddInletPort(carrierNode[noteNumber], 2, SoundFormat.Stereo);
            block.AddOutletPort(carrierNode[noteNumber], 2, SoundFormat.Stereo);
            block.SetFloat<ModulatableSineOscillatorNodeJob.Params, NoProvs, ModulatableSineOscillatorNodeJob>(
                carrierNode[noteNumber], 
                ModulatableSineOscillatorNodeJob.Params.Frequency, math.pow(2, (noteNumber - 69) / 12.0f) * 440.0f
            );

            carrierADSNode[noteNumber] = block.CreateDSPNode<AttackDecaySustainNodeJob.Params, NoProvs, AttackDecaySustainNodeJob>();
            block.AddInletPort(carrierADSNode[noteNumber], 2, SoundFormat.Stereo);
            block.AddOutletPort(carrierADSNode[noteNumber], 2, SoundFormat.Stereo);
            block.SetFloat<AttackDecaySustainNodeJob.Params, NoProvs, AttackDecaySustainNodeJob>(
                carrierADSNode[noteNumber], 
                AttackDecaySustainNodeJob.Params.Attack, 0.1f
            );

            block.SetFloat<AttackDecaySustainNodeJob.Params, NoProvs, AttackDecaySustainNodeJob>(
                carrierADSNode[noteNumber], 
                AttackDecaySustainNodeJob.Params.Decay, 1.0f
            );

            block.SetFloat<AttackDecaySustainNodeJob.Params, NoProvs, AttackDecaySustainNodeJob>(
                carrierADSNode[noteNumber], 
                AttackDecaySustainNodeJob.Params.Sustain, 0.0f
            );

            block.Connect(modulatorNode[noteNumber], 0, modulatorADSNode[noteNumber], 0);
            block.Connect(modulatorADSNode[noteNumber], 0, carrierNode[noteNumber], 0);
            block.Connect(carrierNode[noteNumber], 0, carrierADSNode[noteNumber], 0);
            block.Connect(carrierADSNode[noteNumber], 0, graph.RootDSP, 0);

            block.Complete();
        };

        MidiMaster.noteOffDelegate += (MidiChannel channel, int noteNumber) => {
            var block = graph.CreateCommandBlock();

            block.ReleaseDSPNode(modulatorNode[noteNumber]);
            block.ReleaseDSPNode(carrierNode[noteNumber]);

            block.Complete();
        };
    }

    void OnDestroy() {
        _outputHandle.Dispose();
    }
}
