using Unity.Audio;
using Unity.Burst;

[BurstCompile]
struct MonoToStereoNode : IAudioKernel<MonoToStereoNode.Params, MonoToStereoNode.Providers>
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

    public void Execute(ref ExecuteContext<MonoToStereoNode.Params, MonoToStereoNode.Providers> ctx)
    {
        var inputBuffer = ctx.Inputs.GetSampleBuffer(0);
        var src = inputBuffer.Buffer;
        var outputBuffer = ctx.Outputs.GetSampleBuffer(0);
        var dst = outputBuffer.Buffer;
        var numChannels = outputBuffer.Channels;
        var sampleFrames = outputBuffer.Samples;

        int offset = 0;
        for (int n = 0; n < sampleFrames; n++)
        {
            for (int c = 0; c < numChannels; c++) {
                dst[offset++] = src[n];
            }
        }
    }

    public void Initialize()
    {
    }
}
