﻿using System;
using System.Collections.Generic;

namespace Brimstone
{
	public class Minion : Entity, IMinion
	{
		public Minion(Minion cloneFrom) : base(cloneFrom) { }
		public Minion(Game game, Card card, Dictionary<GameTag, int?> tags = null) : base(game, card, tags) {
			this[GameTag.HEALTH] = card[GameTag.HEALTH];
		}

		public IPlayable Play() {
			// TODO: Might not be CurrentPlayer
			Game.ActionQueue.Enqueue(CardBehaviour.Play(Game.CurrentPlayer, this));
			return (IPlayable)(Entity)Game.ActionQueue.Process()[0];
		}

		public void Damage(int amount) {
			Game.ActionQueue.Enqueue(CardBehaviour.Damage(this, amount));
			Game.ActionQueue.Process();
		}

		public void CheckForDeath() {
			if (this[GameTag.HEALTH] <= 0) {
				Console.WriteLine(this + " dies!");
				Game.Opponent.Board.Remove(this);
				Game.ActionQueue.Enqueue(Card.Behaviour.Deathrattle);
				Game.ActionQueue.Process();
				this[GameTag.ZONE] = (int)Zone.GRAVEYARD;
				this[GameTag.ZONE_POSITION] = 0;
				this[GameTag.DAMAGE] = 0;
			}
		}

		public override string ToString() {
			string s = Card.Name + " (Health=" + Tags[GameTag.HEALTH] + ", ";
			foreach (var tag in Tags) {
				s += tag.Key + ": " + tag.Value + ", ";
			}
			return s.Substring(0, s.Length - 2) + ")";
		}

		public override object Clone() {
			return new Minion(this);
		}
	}
}