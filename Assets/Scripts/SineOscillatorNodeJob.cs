using Unity.Burst;
using Unity.Experimental.Audio;

enum NoProvs
{
}

[BurstCompile]
struct SineOscillatorNodeJob : IAudioJob<SineOscillatorNodeJob.Params, NoProvs>
{
    double phase;

    public enum Params
    {
        Frequency
    }

    public void Init(ParameterData<SineOscillatorNodeJob.Params> parameters)
    {
    }

    public void Execute(ref ExecuteContext<SineOscillatorNodeJob.Params, NoProvs> ctx)
    {
        var outputBuffer = ctx.Outputs.GetSampleBuffer(0);
        var numChannels = outputBuffer.Channels;
        var sampleFrames = outputBuffer.Samples;
        var dst = outputBuffer.Buffer;

        int offset = 0;
        for (uint n = 0; n < sampleFrames; n++)
        {
            for (uint c = 0; c < numChannels; c++) {
                dst[offset++] = (float)Unity.Mathematics.math.sin(phase);
            }

            phase += ctx.Parameters.GetFloat(Params.Frequency, n) * 2 * Unity.Mathematics.math.PI / ctx.SampleRate;
        }
    }
}
