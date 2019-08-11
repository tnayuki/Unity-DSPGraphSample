using Unity.Audio;
using Unity.Burst;

[BurstCompile]
struct VCANode : IAudioKernel<VCANode.Params, VCANode.Providers>
{
    public enum Params
    {
    }

    public enum Providers
    {
    }

    public void Dispose()
    {
    }

    public void Execute(ref ExecuteContext<VCANode.Params, VCANode.Providers> ctx)
    {
        var sigBuffer = ctx.Inputs.GetSampleBuffer(0);
        var modBuffer = ctx.Inputs.GetSampleBuffer(1);
        var sig = sigBuffer.Buffer;
        var mod = modBuffer.Buffer;

        var outputBuffer = ctx.Outputs.GetSampleBuffer(0);
        var output = outputBuffer.Buffer;

        var sampleFrames = outputBuffer.Samples;

        for (int n = 0; n < sampleFrames; n++)
        {
            output[n] = sig[n] * mod[n] / 10.0f;
        }
    }

    public void Initialize()
    {
    }
}
