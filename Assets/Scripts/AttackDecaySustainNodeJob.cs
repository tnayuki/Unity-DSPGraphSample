using Unity.Burst;
using Unity.Experimental.Audio;
using Unity.Mathematics;

[BurstCompile]
struct AttackDecaySustainNodeJob : IAudioJob<AttackDecaySustainNodeJob.Params, NoProvs>
{
    bool started;
    ulong startDSPClock;

    public enum Params
    {
        Attack,
        Decay,
        Sustain
    }

    public void Init(ParameterData<Params> parameters)
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
        for (uint n = 0; n < sampleFrames; n++)
        {
            var attackTime = ctx.Parameters.GetFloat(Params.Attack, n);
            var attackRate = 1.0 / attackTime;

            var decayTime = ctx.Parameters.GetFloat(Params.Decay, n);
            var decayRate = 1.0 / decayTime;

            float level;

            if (!started) {
                startDSPClock = ctx.DSPClock;
                started = true;
            }

            var time = (double)(ctx.DSPClock - startDSPClock + n) / (double)ctx.SampleRate;

            if (time < attackTime)
            {
                level = (float)(time * attackRate);
            }
            else if (time < attackTime + decayTime)
            {
                level = (float)(1 - (time - attackTime) * decayRate * (1 - ctx.Parameters.GetFloat(Params.Sustain, n)));
            }
            else
            {
                level = ctx.Parameters.GetFloat(Params.Sustain, n);
            }

            for (uint c = 0; c < numChannels; c++) {
                dst[offset] = src[offset] * level;
                offset++;
            }
        }
    }
}
