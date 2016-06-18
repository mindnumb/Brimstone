﻿using System;

namespace Brimstone
{
	public abstract partial class CanBeDamaged : Entity
	{
		public void Hit(int amount) {
			Game.ActionQueue.Enqueue(CardBehaviour.Damage(this, amount));
		}

		public void CheckForDeath() {
			if (Health <= 0) {
				Console.WriteLine(Card.Name + " dies!");
				((Player)Controller).Graveyard.MoveTo(this);
				Damage = 0;
				Game.ActionQueue.Enqueue(Card.Behaviour.Deathrattle);
			}
		}
	}
}
