
namespace Celeste64;

/// <summary>
/// Implementation From:
/// https://github.com/orangeduck/Spring-It-On
/// </summary>
public static class SpringPhysics
{
	private const float Epsilon = float.Epsilon;

	private static float HalflifeToDamping(float halflife)
	{
		return (4.0f * 0.69314718056f) / (halflife + Epsilon);
	}

	private static float FrequencyToStiffness(float frequency)
	{
		return Calc.Squared(2.0f * Calc.PI * frequency);
	}

	private static float FastAtan(float x)
	{
		float z = MathF.Abs(x);
		float w = z > 1.0f ? 1.0f / z : z;
		float y = (Calc.PI / 4.0f) * w - w * (w - 1) * (0.2447f + 0.0663f * w);
		return MathF.CopySign(z > 1.0f ? Calc.PI / 2.0f - y : y, x);
	}

	private static float FastNegExp(float x)
	{
		return 1.0f / (1.0f + x + 0.48f * x * x + 0.235f * x * x * x);
	}

	public static float Calculate(float x, float vel, float xGoal, float velGoal, float frequency, float halflife)
	{
		float g = xGoal;
		float q = velGoal;
		float s = FrequencyToStiffness(frequency);
		float d = HalflifeToDamping(halflife);
		float c = g + (d * q) / (s + Epsilon);
		float y = d / 2.0f;
		float dt = Time.Delta;

		if (MathF.Abs(s - (d * d) / 4.0f) < Epsilon) // Critically Damped
		{
			float j0 = x - c;
			float j1 = vel + j0 * y;

			float eydt = FastNegExp(y * dt);

			vel = -y * j0 * eydt - y * dt * j1 * eydt + j1 * eydt;
		}
		else if (s - (d * d) / 4.0f > 0.0) // Under Damped
		{
			float w = MathF.Sqrt(s - (d * d) / 4.0f);
			float j = MathF.Sqrt(Calc.Squared(vel + y * (x - c)) / (w * w + Epsilon) + Calc.Squared(x - c));
			float p = FastAtan((vel + (x - c) * y) / (-(x - c) * w + Epsilon));

			j = (x - c) > 0.0f ? j : -j;

			float eydt = FastNegExp(y * dt);

			vel = -y * j * eydt * MathF.Cos(w * dt + p) - w * j * eydt * MathF.Sin(w * dt + p);
		}
		else if (s - (d * d) / 4.0f < 0.0) // Over Damped
		{
			float y0 = (d + MathF.Sqrt(d * d - 4 * s)) / 2.0f;
			float y1 = (d - MathF.Sqrt(d * d - 4 * s)) / 2.0f;
			float j1 = (c * y0 - x * y0 - vel) / (y1 - y0);
			float j0 = x - j1 - c;

			float ey0dt = FastNegExp(y0 * dt);
			float ey1dt = FastNegExp(y1 * dt);

			vel = -y0 * j0 * ey0dt - y1 * j1 * ey1dt;
		}

		return vel;
	}
}
