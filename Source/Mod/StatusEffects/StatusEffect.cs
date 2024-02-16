namespace Celeste64.Mod;

public abstract class StatusEffect
{
	public virtual Player? Player { get; set; }
	public virtual World? World { get; set; }
	public virtual bool RemoveOnReapply { get { return true; } }

	public virtual float Duration { get; set; } = 10;
	public bool RemoveAfterDuration = false;

	public StatusEffect() { }

	public virtual void OnStatusEffectAdded() { }
	public virtual void OnStatusEffectRemoved() { }

	public virtual void Update(float deltaTime) { }

	public virtual void OnPlayerKilled() { }

	public void UpdateDuration(float deltaTime)
	{
		if(RemoveAfterDuration)
		{
			Duration -= deltaTime;
			if (Duration <= 0 && Player != null)
			{
				Player.RemoveStatusEffect(this);
			}
		}
	}
}