using Unity.Audio;
using Unity.Burst;
using Unity.Mathematics;

// [BurstCompile]
struct VCONode : IAudioKernel<VCONode.Params, VCONode.Providers>
{
    private double _phase;

    public enum Params
    {
    }

    public enum Providers
    {
    }


    public void Dispose()
    {
    }

    public void Execute(ref ExecuteContext<Params, Providers> ctx)
    {
        var cvBuffer = ctx.Inputs.GetSampleBuffer(0);
        var outputBuffer = ctx.Outputs.GetSampleBuffer(0);
        var numChannels = outputBuffer.Channels;
        var sampleFrames = outputBuffer.Samples;
        var cv = cvBuffer.Buffer;
        var dst = outputBuffer.Buffer;

        for (int n = 0; n < sampleFrames; n++)
        {
            dst[n] = (float)math.sin(_phase);

            _phase += 110.0f * math.pow(2, cv[n]) * 2.0f * math.PI / (float)ctx.SampleRate;
        }
    }

    public void Initialize()
    {
    }
}
