using Unity.Audio;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
struct ADSRNode : IAudioKernel<ADSRNode.Params, ADSRNode.Providers>
{
    bool started;
    long startDSPClock;
    long stopDSPClock;

    public enum Params
    {
        Attack,
        Decay,
        Sustain,
        Release
    }
 
    public enum Providers
    {
    }
 
    public void Dispose()
    {
    }

    public void Execute(ref ExecuteContext<Params, Providers> ctx)
    {
        var gateBuffer = ctx.Inputs.GetSampleBuffer(0);
        var outputBuffer = ctx.Outputs.GetSampleBuffer(0);
        var sampleFrames = outputBuffer.Samples;
        var gate = gateBuffer.Buffer;
        var dst = outputBuffer.Buffer;

        for (int n = 0; n < sampleFrames; n++)
        {
            var attackTime = ctx.Parameters.GetFloat(Params.Attack, n);
            var attackRate = 10.0f / attackTime;

            var decayTime = ctx.Parameters.GetFloat(Params.Decay, n);

            float level;

            if (started && gate[n] <= math.FLT_MIN_NORMAL)
            {
                started = false;
                stopDSPClock = ctx.DSPClock + n;
            }
            else if (!started && gate[n] > math.FLT_MIN_NORMAL)
            {
                started = true;
                startDSPClock = ctx.DSPClock + n;
            }

            var time = (float)(ctx.DSPClock + n - startDSPClock) / (float)ctx.SampleRate;

            if (started && time < attackTime)
            {
                level = time * attackRate;
            }
            else if (started && time < attackTime + decayTime)
            {
                level = 10.0f - (time - attackTime) * (10.0f - ctx.Parameters.GetFloat(Params.Sustain, n)) / decayTime;
            }
            else if (started)
            {
                level = ctx.Parameters.GetFloat(Params.Sustain, n);
            }
            else
            {
                level = ctx.Parameters.GetFloat(Params.Sustain, n) - (time - (stopDSPClock - startDSPClock) / (float)ctx.SampleRate) * ctx.Parameters.GetFloat(Params.Sustain, n) / ctx.Parameters.GetFloat(Params.Release, n);
            }

            dst[n] = math.max(0.0f, level);
        }
    }

    public void Initialize()
    {
    }
}
