﻿namespace Celeste64;

public abstract class StatusEffect
{
	public Player? Player { get; internal set; }
	public World? World { get; internal set; }
	public virtual bool RemoveOnReapply { get { return true; } }

	public float Duration { get; set; } = 10;
	public bool RemoveAfterDuration = false;

	protected StatusEffect() { }

	public virtual void OnStatusEffectAdded() { }
	public virtual void OnStatusEffectRemoved() { }

	public virtual void Update(float deltaTime) { }

	public virtual void OnPlayerKilled() { }


	internal void UpdateDuration(float deltaTime)
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