/*
 * TJS2 CSharp
 */

using Tjs2.Sharpen;

namespace Tjs2.Engine
{
	public class ExprNode
	{
		public int OpCode { get; set; }

		public int Position { get; set; }

		private AList<ExprNode> mNodes;

		private Variant mVal;

		public ExprNode()
		{
			//mOp = 0;
			//mNodes = null;
			//mVal = null;
			Position = -1;
		}

		~ExprNode()
		{
			if (mNodes != null)
			{
				int count = mNodes.Count;
				for (int i = 0; i < count; i++)
				{
					ExprNode node = mNodes[i];
				    node?.Clear();
				}
				mNodes.Clear();
				mNodes = null;
			}
			if (mVal != null)
			{
				mVal.Clear();
				mVal = null;
			}
		}

		public void SetValue(Variant val)
		{
			if (mVal == null)
			{
				mVal = new Variant(val);
			}
			else
			{
				mVal.CopyRef(val);
			}
		}

		public void Add(ExprNode node)
		{
			if (mNodes == null)
			{
				mNodes = new AList<ExprNode>();
			}
			mNodes.AddItem(node);
		}

		public Variant GetValue()
		{
			return mVal;
		}

		public ExprNode GetNode(int index)
		{
			if (mNodes == null)
			{
				return null;
			}
			else
			{
				if (index < mNodes.Count)
				{
					return mNodes[index];
				}
				else
				{
					return null;
				}
			}
		}

		public int GetSize()
		{
			if (mNodes == null)
			{
				return 0;
			}
			else
			{
				return mNodes.Count;
			}
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		public void AddArrayElement(Variant val)
		{
			string ss_add = "add";
			Variant[] args = new Variant[1];
			args[0] = val;
			mVal.AsObjectClosure().FuncCall(0, ss_add, null, args, null);
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		public void AddDictionaryElement(string name, Variant val)
		{
			mVal.AsObjectClosure().PropSet(Interface.MEMBERENSURE, name, val, null);
		}

		public void Clear()
		{
			if (mNodes != null)
			{
				int count = mNodes.Count;
				for (int i = 0; i < count; i++)
				{
					ExprNode node = mNodes[i];
				    node?.Clear();
				}
				mNodes.Clear();
				mNodes = null;
			}
			if (mVal != null)
			{
				mVal.Clear();
				mVal = null;
			}
		}
	}
}
