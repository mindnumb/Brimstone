﻿ using System;
using System.Collections.Generic;
using System.Linq;
 using Brimstone.Exceptions;

namespace Brimstone.Entities
{
	public class HeroPower : CanTarget
	{
		public HeroPower(HeroPower cloneFrom) : base(cloneFrom) { }
		internal HeroPower(Card card, Dictionary<GameTag, int> tags = null) : base(card, tags) { }

		public override IEnumerable<ICharacter> ValidTargets => GetValidAbilityTargets();

		protected internal override bool MeetsGenericTargetingRequirements(ICharacter target) {
			Minion minion = target as Minion;

			// Spells and hero powers can't target CantBeTargetedByAbilities minions
			return (minion != null && minion.CantBeTargetedByAbilities ? false : base.MeetsGenericTargetingRequirements(target));
		}

		public HeroPower Activate(ICharacter target = null) {
			if (Game.Player1.Choice != null || Game.Player2.Choice != null)
				throw new ChoiceException();

			Target = target;
			try {
				return (HeroPower)Game.RunActionBlock(BlockType.PLAY, this, Actions.UseHeroPower(this), Target);
			}
			// Action was probably cancelled causing an uninitialized ActionResult to be returned
			catch (NullReferenceException) {
				return null;
			}
		}

		public override object Clone() {
			return new HeroPower(this);
		}
	}
}
