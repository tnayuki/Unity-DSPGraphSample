using Unity.Audio;
using Unity.Burst;

[BurstCompile]
struct MIDICVNode : IAudioKernel<MIDICVNode.Params, MIDICVNode.Providers>
{
    private bool _noteOn;
    private float _lastNote;

    public enum Params
    {
        Note
    }

    public enum Providers
    {
        Note
    }

    public void Dispose()
    {
    }

    public void Execute(ref ExecuteContext<MIDICVNode.Params, MIDICVNode.Providers> ctx)
    {
        var cvBuffer = ctx.Outputs.GetSampleBuffer(0);
        var gateBuffer = ctx.Outputs.GetSampleBuffer(1);

        var numChannels = cvBuffer.Channels;
        var sampleFrames = cvBuffer.Samples;
        var cv = cvBuffer.Buffer;
        var gate = gateBuffer.Buffer;

        for (int n = 0; n < sampleFrames; n++)
        {
            var note = ctx.Parameters.GetFloat(Params.Note, n);

            _noteOn = note >= 0.0f;
            if (_noteOn) _lastNote = note;

            cv[n] = (_lastNote - 33.0f) / 12.0f;
            gate[n] = _noteOn ? 10.0f : 0.0f;
        }
    }

    public void Initialize()
    {
    }
}
