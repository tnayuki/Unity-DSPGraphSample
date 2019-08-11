using Unity.Audio;
using Unity.Mathematics;
using UnityEngine;

using MidiJack;

public class Demo5 : MonoBehaviour
{
    private AudioOutputHandle _outputHandle;

    void Start()
    {
        var graph = DSPGraph.Create(SoundFormat.Stereo, 2, 1024, 48000);

        var driver = new DefaultDSPGraphDriver { Graph = graph };
        driver.Initialize(2, SoundFormat.Stereo, 48000, 1024);

        _outputHandle = driver.AttachToDefaultOutput();

        var block = graph.CreateCommandBlock();

        var midiCvNode = block.CreateDSPNode<MIDICVNode.Params, MIDICVNode.Providers, MIDICVNode>();
        block.AddOutletPort(midiCvNode, 1, SoundFormat.Mono);
        block.AddOutletPort(midiCvNode, 1, SoundFormat.Mono);
        block.SetFloat<MIDICVNode.Params, MIDICVNode.Providers, MIDICVNode>(midiCvNode, MIDICVNode.Params.Note, -1.0f);

        var vcoNode = block.CreateDSPNode<VCONode.Params, VCONode.Providers, VCONode>();
        block.AddInletPort(vcoNode, 1, SoundFormat.Mono);
        block.AddOutletPort(vcoNode, 1, SoundFormat.Mono);

        var adsrNode = block.CreateDSPNode<ADSRNode.Params, ADSRNode.Providers, ADSRNode>();
        block.AddInletPort(adsrNode, 1, SoundFormat.Mono);
        block.AddOutletPort(adsrNode, 1, SoundFormat.Mono);
        block.SetFloat<ADSRNode.Params, ADSRNode.Providers, ADSRNode>(adsrNode, ADSRNode.Params.Attack, 0.2f);
        block.SetFloat<ADSRNode.Params, ADSRNode.Providers, ADSRNode>(adsrNode, ADSRNode.Params.Decay, 2.0f);
        block.SetFloat<ADSRNode.Params, ADSRNode.Providers, ADSRNode>(adsrNode, ADSRNode.Params.Sustain, 3.0f);
        block.SetFloat<ADSRNode.Params, ADSRNode.Providers, ADSRNode>(adsrNode, ADSRNode.Params.Release, 0.5f);

        var vcaNode = block.CreateDSPNode<VCANode.Params, VCANode.Providers, VCANode>();
        block.AddInletPort(vcaNode, 1, SoundFormat.Mono);
        block.AddInletPort(vcaNode, 1, SoundFormat.Mono);
        block.AddOutletPort(vcaNode, 1, SoundFormat.Mono);

        var monoToStereoNode = block.CreateDSPNode<MonoToStereoNode.Params, MonoToStereoNode.Providers, MonoToStereoNode>();
        block.AddInletPort(monoToStereoNode, 1, SoundFormat.Mono);
        block.AddOutletPort(monoToStereoNode, 2, SoundFormat.Stereo);

        block.Connect(midiCvNode, 0, vcoNode, 0);
        block.Connect(midiCvNode, 1, adsrNode, 0);
        block.Connect(vcoNode, 0, vcaNode, 0);
        block.Connect(adsrNode, 0, vcaNode, 1);
        block.Connect(vcaNode, 0, monoToStereoNode, 0);
        block.Connect(monoToStereoNode, 0, graph.RootDSP, 0);

        block.Complete();

        MidiMaster.noteOnDelegate += (MidiChannel channel, int noteNumber, float velocity) => {
            var noteOnBlock = graph.CreateCommandBlock();

            noteOnBlock.SetFloat<MIDICVNode.Params, MIDICVNode.Providers, MIDICVNode>(midiCvNode, MIDICVNode.Params.Note, (float)noteNumber);

            noteOnBlock.Complete();
        };

        MidiMaster.noteOffDelegate += (MidiChannel channel, int noteNumber) => {
            var noteOffBlock = graph.CreateCommandBlock();

            noteOffBlock.SetFloat<MIDICVNode.Params, MIDICVNode.Providers, MIDICVNode>(midiCvNode, MIDICVNode.Params.Note, -1.0f);

            noteOffBlock.Complete();
        };
    }

    void OnDestroy() {
        _outputHandle.Dispose();
    }
}
