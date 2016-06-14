﻿using System;
using System.Collections.Generic;

namespace Brimstone
{
	public class Game : BaseEntity
	{
		public int NextEntityId = 1;

		public Player Player1 { get; set; }
		public Player Player2 { get; set; }
		public Player CurrentPlayer { get; set; }
		public Player Opponent { get; set; }

		public PowerHistory PowerHistory = new PowerHistory();
		public ActionQueue ActionQueue = new ActionQueue();

		// Required by IEntity
		public Game(Game cloneFrom) : base(cloneFrom) {
			NextEntityId = cloneFrom.NextEntityId;
			Game = this;
			Player1 = (Player)cloneFrom.Player1.Clone();
			Player2 = (Player)cloneFrom.Player2.Clone();
			// Yeah, fix this...
			CurrentPlayer = Player1;
			Opponent = Player2;
			// Change ownership
			foreach (var e in Entities)
				e.Game = this;
			// NOTE: Don't clone PowerHistory!
			ActionQueue.Queue = new Queue<QueueAction>(cloneFrom.ActionQueue.Queue);
			ActionQueue.ResultStack = new Stack<ActionResult>(cloneFrom.ActionQueue.ResultStack);
			ActionQueue.Attach(this);
		}

		public Game(Dictionary<GameTag, int?> tags = null,
					bool PowerHistory = false) : base(null, Cards.Find["Game"], tags) {
			if (PowerHistory) {
				this.PowerHistory.Attach(this);
				this.PowerHistory.Add(new CreateEntity(this) { EntityId = NextEntityId, Tags = tags });
			}
			ActionQueue.Attach(this);
			NextEntityId++;
			Game = this;
		}

		public override string ToString() {
			string s = "Board state: ";
			var players = new List<Player> { Player1, Player2 };
			foreach (var player in players) {
				s += "Player " + player.Card.Id + " - ";
				s += "HAND: ";
				foreach (var entity in player.ZoneHand) {
					s += entity.ToString() + ", ";
				}
				s += "PLAY: ";
				foreach (var entity in player.ZonePlay) {
					s += entity.ToString() + ", ";
				}
			}
			s = s.Substring(0, s.Length - 2) + "\nPower log: ";
			foreach (var item in PowerHistory)
				s += item + "\n";
			return s;
		}

		public IEnumerable<IEntity> Entities {
			get {
				yield return this;
				yield return Player1;
				yield return Player2;
				foreach (var e in Player1.ZoneHand)
					yield return e;
				foreach (var e in Player1.ZonePlay)
					yield return e;
				foreach (var e in Player2.ZoneHand)
					yield return e;
				foreach (var e in Player2.ZonePlay)
					yield return e;
			}
		}

		public void BeginTurn() {
			ActionQueue.Enqueue(CardBehaviour.BeginTurn);
			ActionQueue.Process();
		}

		public override object Clone() {
			return new Game(this);
		}
	}
}