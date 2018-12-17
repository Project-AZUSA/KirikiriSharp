/*
 * TJS2 CSharp
 */

namespace Tjs2.Engine
{
	public class ScriptLineData
	{
		private readonly string mString;

		private IntVector mLineVector;

		private IntVector mLineLengthVector;

		private int mLineOffset;

		private const int CARRIAGE_RETURN = 13;

		private const int LINE_FEED = 10;

		public ScriptLineData(string @string, int offset)
		{
			mString = @string;
			mLineOffset = offset;
		}

		private void GenerateLineVector()
		{
			mLineVector = new IntVector();
			mLineLengthVector = new IntVector();
			int count = mString.Length;
			int lastCR = 0;
			int i;
			for (i = 0; i < count; i++)
			{
				int c = mString[i];
				if (c == CARRIAGE_RETURN || c == LINE_FEED)
				{
					mLineVector.Add(lastCR);
					mLineLengthVector.Add(i - lastCR);
					lastCR = i + 1;
					if ((i + 1) < count)
					{
						c = mString[i + 1];
						if (c == CARRIAGE_RETURN || c == LINE_FEED)
						{
							i++;
							lastCR = i + 1;
						}
					}
				}
			}
			if (i != lastCR)
			{
				mLineVector.Add(lastCR);
				mLineLengthVector.Add(i - lastCR);
			}
		}

		public int GetSrcPosToLine(int pos)
		{
			if (mLineVector == null)
			{
				GenerateLineVector();
			}
			// 2分法によって位置を求める
			int s = 0;
			int e = mLineVector.Size();
			while (true)
			{
				if ((e - s) <= 1)
				{
					return s + mLineOffset;
				}
				int m = s + (e - s) / 2;
				if (mLineVector.Get(m) > pos)
				{
					e = m;
				}
				else
				{
					s = m;
				}
			}
		}

		public int GetLineToSrcPos(int pos)
		{
			if (mLineVector == null)
			{
				GenerateLineVector();
			}
			return mLineVector.Get(pos);
		}

		public string GetLine(int line)
		{
			if (mLineVector == null)
			{
				GenerateLineVector();
			}
			int start = mLineVector.Get(line);
			int length = mLineLengthVector.Get(line);
			return Sharpen.Runtime.Substring(mString, start, start + length);
		}

		public int GetMaxLine()
		{
			if (mLineVector == null)
			{
				GenerateLineVector();
			}
			return mLineVector.Size();
		}
	}
}
