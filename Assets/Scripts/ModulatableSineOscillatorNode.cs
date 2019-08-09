using Unity.Audio;
using Unity.Burst;

[BurstCompile]
struct ModulatableSineOscillatorNodeJob : IAudioKernel<ModulatableSineOscillatorNodeJob.Params, NoProvs>
{
    double phase;

    public enum Params
    {
        Frequency
    }

    public void Dispose()
    {
    }

    public void Execute(ref ExecuteContext<Params, NoProvs> ctx)
    {
        var inputBuffer = ctx.Inputs.GetSampleBuffer(0);
        var outputBuffer = ctx.Outputs.GetSampleBuffer(0);
        var numChannels = outputBuffer.Channels;
        var sampleFrames = outputBuffer.Samples;
        var src = inputBuffer.Buffer;
        var dst = outputBuffer.Buffer;

        int offset = 0;
        for (int n = 0; n < sampleFrames; n++)
        {
            for (uint c = 0; c < numChannels; c++) {
                dst[offset++] = (float)Unity.Mathematics.math.sin(phase);
            }

            phase += ctx.Parameters.GetFloat(Params.Frequency, n) * 2 * Unity.Mathematics.math.PI / ctx.SampleRate;
            phase += src[offset - 2];
        }
    }

    public void Initialize()
    {
    }
}
