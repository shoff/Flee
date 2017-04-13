using Ciloci.Flee.PerCederberg.Grammatica.Runtime.RE;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;

namespace Ciloci.Flee.PerCederberg.Grammatica.Runtime
{
	internal class RegExpTokenMatcher : TokenMatcher
	{
		private TokenPattern pattern;

		private RegExp regExp;

		private Matcher matcher;

		public RegExpTokenMatcher(TokenPattern pattern, bool ignoreCase, LookAheadReader input)
		{
			this.pattern = pattern;
			this.regExp = new RegExp(pattern.Pattern, ignoreCase);
			this.matcher = this.regExp.Matcher(input);
		}

		public void Reset(LookAheadReader input)
		{
			this.matcher.Reset(input);
		}

		public TokenPattern GetPattern()
		{
			return this.pattern;
		}

		public override TokenPattern GetMatchedPattern()
		{
			bool flag = this.matcher == null || this.matcher.Length() <= 0;
			TokenPattern GetMatchedPattern;
			if (flag)
			{
				GetMatchedPattern = null;
			}
			else
			{
				GetMatchedPattern = this.pattern;
			}
			return GetMatchedPattern;
		}

		public override int GetMatchedLength()
		{
			return Conversions.ToInteger(Interaction.IIf(this.matcher == null, 0, this.matcher.Length()));
		}

		public bool Match()
		{
			return this.matcher.MatchFromBeginning();
		}

		public override string ToString()
		{
			return (this.pattern.ToString() + "\n" + (this.regExp.ToString() ?? "") + "\n") ?? "";
		}
	}
}