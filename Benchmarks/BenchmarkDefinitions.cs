﻿using System.Collections.Generic;
using System.Linq;
using Brimstone;

namespace Brimstone.Benchmark
{
	internal partial class Benchmarks {
		// ======================================================
		// =============== BENCHMARK DEFINITIONS ================
		// ======================================================

		public const int DefaultIterations = 100000;

		public void LoadDefinitions() {
			Tests = new Dictionary<string, Test>() {
				{ "RawCloneDeepCopy", new Test("Raw cloning speed; naive deep copy (68 entities)", Test_RawClone_DeepCopy) },
				{ "RawCloneCopyOnWrite", new Test("Raw cloning speed; copy-on-write (68 entities)", Test_RawClone_CopyOnWrite) },
				{ "BoomBotPreHit", new Test("Boom Bot pre-hit cloning test; copy-on-write; 5 RC + 2 BB per side", Test_BoomBotPreHit) },
				{ "BoomBotPreDeathrattleCopyOnWrite", new Test("Boom Bot pre-deathrattle cloning test; copy-on-write; 5 RC + 2 BB per side", Test_BoomBotPreDeathrattle_CopyOnWrite) },
				{ "BoomBotPreDeathrattleDeepCopy", new Test("Boom Bot pre-deathrattle cloning test; naive deep copy; 5 RC + 2 BB per side", Test_BoomBotPreDeathrattle_DeepCopy) },
				{ "BoomBotPreDeathrattleDeepCopyNoZoneCache", new Test("Boom Bot pre-deathrattle cloning test; naive deep copy; no zone caching; 5 RC + 2 BB per side", Test_BoomBotPreDeathrattle_DeepCopy_NoZoneCache) },
				{ "BoomBotUniqueStatesNS", new Test("Boom Bot hit; fuzzy unique states; Naive; 5 BR + 2 BB per side", Test_BoomBotUniqueStatesNS, Default_Setup2, 1) },
				{ "BoomBotUniqueStatesDSF", new Test("Boom Bot hit; fuzzy unique states; DSF; 5 BR + 2 BB per side", Test_BoomBotUniqueStatesDSF, Default_Setup2, 1) },
				{ "BoomBotUniqueStatesBSF", new Test("Boom Bot hit; fuzzy unique states; BSF; 5 BR + 2 BB per side", Test_BoomBotUniqueStatesBSF, Default_Setup2, 1) },
				{ "ArcaneMissiles2UniqueStatesNS", new Test("Arcane Missiles (2); fuzzy unique game states; Naive; 5 BR + 2 BB per side", Test_2AMUniqueStatesNS, Default_Setup2, 1) },
				{ "ArcaneMissiles2UniqueStatesDSF", new Test("Arcane Missiles (2); fuzzy unique game states; DSF; 5 BR + 2 BB per side", Test_2AMUniqueStatesDSF, Default_Setup2, 1) },
				{ "ArcaneMissiles2UniqueStatesDSFNoZoneCache", new Test("Arcane Missiles (2); fuzzy unique game states; DSF; no zone caching; 5 BR + 2 BB per side", Test_2AMUniqueStatesDSFNoZoneCache, Default_Setup2, 1) },
				{ "ArcaneMissiles2UniqueStatesBSF", new Test("Arcane Missiles (2); fuzzy unique game states; BSF; 5 BR + 2 BB per side", Test_2AMUniqueStatesBSF, Default_Setup2, 1) },
				{ "ArcaneMissiles3UniqueStatesBSF", new Test("Arcane Missiles (3); fuzzy unique game states; BSF; 5 BR + 2 BB per side", Test_3AMUniqueStatesBSF, Default_Setup2, 1) },
			};
		}

		// ======================================================
		// ===============    BENCHMARK CODE    =================
		// ======================================================

		public static Game Default_Setup() {
			return NewScenarioGame(MaxMinions: 7, NumBoomBots: 2, FillMinion: "River Crocolisk");
		}
		public static Game Default_Setup2() {
			return NewScenarioGame(MaxMinions: 7, NumBoomBots: 2, FillMinion: "Bloodfen Raptor", FillDeck: false);
		}


		private void _clone(Game g, int it) {
			for (int i = 0; i < it; i++)
				g.CloneState();
		}
		public void Test_RawClone_CopyOnWrite(Game g, int it) {
			Settings.CopyOnWrite = true;
			_clone(g, it);
		}
		public void Test_RawClone_DeepCopy(Game g, int it) {
			Settings.CopyOnWrite = false;
			Test_RawClone_CopyOnWrite(g, it);
			_clone(g, it);
			Settings.CopyOnWrite = true;
		}

		public void Test_BoomBotPreHit(Game g, int it) {
			var BoomBotId = g.Player1.Board.First(t => t.Card.Name == "Boom Bot").Id;
			for (int i = 0; i < it; i++) {
				Game cloned = (Game)g.CloneState();
				((Minion)cloned.Entities[BoomBotId]).Hit(1);
			}
		}
		private void _boomBotPreDeathrattle(Game g, int it) {
			// Capture after Boom Bot has died but before Deathrattle executes
			var BoomBot = g.Player1.Board.First(t => t.Card.Name == "Boom Bot") as Minion;
			g.ActionQueue.OnAction += (o, e) => {
				ActionQueue queue = o as ActionQueue;
				if (e.Action is Death && e.Source == BoomBot) {
					for (int i = 0; i < it; i++) {
						Game cloned = (Game)g.CloneState();
						cloned.ActionQueue.ProcessAll();
					}
				}
			};
			BoomBot.Hit(1);
		}
		public void Test_BoomBotPreDeathrattle_DeepCopy(Game g, int it) {
			Settings.CopyOnWrite = false;
			_boomBotPreDeathrattle(g, it);
			Settings.CopyOnWrite = true;
		}
		public void Test_BoomBotPreDeathrattle_DeepCopy_NoZoneCache(Game g, int it) {
			Settings.ZoneCaching = false;
			Settings.CopyOnWrite = false;
			_boomBotPreDeathrattle(g, it);
			Settings.CopyOnWrite = true;
			Settings.ZoneCaching = true;
		}
		public void Test_BoomBotPreDeathrattle_CopyOnWrite(Game g, int it) {
			Settings.CopyOnWrite = true;
			_boomBotPreDeathrattle(g, it);
		}

		private void _boomBotUniqueStates(Game g, int it, ITreeSearcher search) {
			var BoomBot = g.CurrentPlayer.Board.First(t => t.Card.Name == "Boom Bot") as Minion;
			var tree = GameTree.BuildFor(
				Game: g,
				SearchMode: search,
				Action: () => {
					BoomBot.Hit(1);
				}
			);
		}
		public void Test_BoomBotUniqueStatesNS(Game g, int it) {
			_boomBotUniqueStates(g, it, new NaiveTreeSearch());
		}

		public void Test_BoomBotUniqueStatesDSF(Game g, int it) {
			_boomBotUniqueStates(g, it, new DepthFirstTreeSearch());
		}

		public void Test_BoomBotUniqueStatesBSF(Game g, int it) {
			_boomBotUniqueStates(g, it, new BreadthFirstTreeSearch());
		}

		private void _missilesUniqueStates(Game g, int it, int missiles, ITreeSearcher search) {
			Cards.FromName("Arcane Missiles").Behaviour.Battlecry = Actions.Damage(Actions.RandomOpponentCharacter, 1) * missiles;
			Cards.FromName("Boom Bot").Behaviour.Deathrattle = Actions.Damage(Actions.RandomOpponentMinion, Actions.RandomAmount(1, 4));

			var ArcaneMissiles = g.CurrentPlayer.Give("Arcane Missiles");
			var tree = GameTree.BuildFor(
				Game: g,
				SearchMode: search,
				Action: () => {
					ArcaneMissiles.Play();
				}
			);
		}

		public void Test_2AMUniqueStatesNS(Game g, int it) {
			_missilesUniqueStates(g, it, 2, new NaiveTreeSearch());
		}

		public void Test_2AMUniqueStatesDSF(Game g, int it) {
			_missilesUniqueStates(g, it, 2, new DepthFirstTreeSearch());
		}

		public void Test_2AMUniqueStatesDSFNoZoneCache(Game g, int it) {
			Settings.ZoneCaching = false;
			_missilesUniqueStates(g, it, 2, new DepthFirstTreeSearch());
			Settings.ZoneCaching = true;
		}

		public void Test_2AMUniqueStatesBSF(Game g, int it) {
			_missilesUniqueStates(g, it, 2, new BreadthFirstTreeSearch());
		}

		public void Test_3AMUniqueStatesBSF(Game g, int it) {
			_missilesUniqueStates(g, it, 3, new BreadthFirstTreeSearch());
		}
	}
}
