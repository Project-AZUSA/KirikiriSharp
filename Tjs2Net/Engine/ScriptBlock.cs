/*
 * TJS2 CSharp
 */

using System.Collections.Generic;
using System.Text;
using Tjs2.Sharpen;

namespace Tjs2.Engine
{
	public class ScriptBlock : SourceCodeAccessor
	{
		private const bool D_IS_DISASSEMBLE = false;

		private static readonly string NO_SCRIPT = "no script";

		private Tjs mOwner;

		private InterCodeObject mTopLevelObject;

		private AList<WeakReference<InterCodeObject>> mInterCodeObjectList;

        public List<WeakReference<InterCodeObject>> Objects => mInterCodeObjectList;

	    private string mName;

		private int mLineOffset;

		private string mScript;

		private ScriptLineData mLineData;

		public ScriptBlock(Tjs owner, string name, int lineoffset, string script, ScriptLineData
			 linedata)
		{
			// a class for managing the script block
			// 以下の4つは实行时にいるかな、名前以外はエラー発生时に必要になるだけだろうけど。
			mOwner = owner;
			mName = name;
			mLineOffset = lineoffset;
			mScript = script;
			mLineData = linedata;
			mOwner.AddScriptBlock(this);
		}

		public virtual void SetObjects(InterCodeObject toplevel, AList<InterCodeObject> objs
			)
		{
			mTopLevelObject = toplevel;
			mInterCodeObjectList = new AList<WeakReference<InterCodeObject>>(objs.Count);
			int count = objs.Count;
			for (int i = 0; i < count; i++)
			{
				mInterCodeObjectList.AddItem(new WeakReference<InterCodeObject>(objs[i]));
			}
		}

		public virtual void SetObjects(InterCodeObject toplevel, InterCodeObject[] objs, 
			int count)
		{
			mTopLevelObject = toplevel;
			mInterCodeObjectList = new AList<WeakReference<InterCodeObject>>(objs.Length);
			for (int i = 0; i < count; i++)
			{
				mInterCodeObjectList.AddItem(new WeakReference<InterCodeObject>(objs[i]));
				objs[i] = null;
			}
		}

		public ScriptBlock(Tjs owner)
		{
			mOwner = owner;
			// Java で初期值となる初期化は省略
			//mScript = null;
			//mName = null;
			//mInterCodeContext = null;
			//mTopLevelContext = null;
			//mLexicalAnalyzer = null;
			//mUsingPreProcessor = false;
			//mLineOffset = 0;
			//mCompileErrorCount = 0;
			//mNode = null;
			mOwner.AddScriptBlock(this);
		}

		~ScriptBlock()
		{
			if (mTopLevelObject != null)
			{
				mTopLevelObject = null;
			}
			mOwner.RemoveScriptBlock(this);
			if (mScript != null)
			{
				mScript = null;
			}
			if (mName != null)
			{
				mName = null;
			}
            //try
            //{
            //    base.Finalize();
            //}
            //catch
            //{
            //}
		}

		public virtual void Compact()
		{
			if (Tjs.IsLowMemory)
			{
				mScript = null;
				mLineData = null;
				int count = mInterCodeObjectList.Count;
				for (int i = 0; i < count; i++)
				{
					InterCodeObject v = mInterCodeObjectList[i].Get();
					if (v != null)
					{
						v.Compact();
					}
				}
			}
		}

		public virtual int SrcPosToLine(int pos)
		{
			if (mLineData == null)
			{
				return 0;
			}
			return mLineData.GetSrcPosToLine(pos);
		}

		public virtual Tjs GetTJS()
		{
			return mOwner;
		}

		public virtual string GetName()
		{
			return mName;
		}

		public virtual int GetLineOffset()
		{
			return mLineOffset;
		}

		private void CompactInterCodeObjectList()
		{
			// なくなっているオブジェクトを消す
			int count = mInterCodeObjectList.Count;
			for (int i = count - 1; i >= 0; i--)
			{
				if (mInterCodeObjectList[i].Get() == null)
				{
					mInterCodeObjectList.Remove(i);
				}
			}
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		/// <exception cref="CompileException"></exception>
		public virtual void ExecuteTopLevel(Variant result, Dispatch2 context)
		{
			// compiles text and executes its global level scripts.
			// the script will be compiled as an expression if isexpressn is true.
			// 逆アセンブル
			// execute global level script
			ExecuteTopLevelScript(result, context);
			int context_count = mInterCodeObjectList.Count;
			if (context_count != 1)
			{
				// this is not a single-context script block
				// (may hook itself)
				// release all contexts and global at this time
				InterCodeObject toplevel = mTopLevelObject;
				if (mTopLevelObject != null)
				{
					mTopLevelObject = null;
				}
				Remove(toplevel);
			}
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual void ExecuteTopLevelScript(Variant result, Dispatch2 context)
		{
			if (mTopLevelObject != null)
			{
				mTopLevelObject.FuncCall(0, null, result, null, context);
			}
		}

		public virtual string GetLineDescriptionString(int pos)
		{
			// get short description, like "mainwindow.tjs(321)"
			// pos is in character count from the first of the script
			StringBuilder builer = new StringBuilder(512);
			int line = SrcPosToLine(pos) + 1;
			if (mName != null)
			{
				builer.Append(mName);
			}
			else
			{
				builer.Append("anonymous@");
				builer.Append(ToString());
			}
			builer.Append('(');
			builer.Append(line.ToString());
			builer.Append(')');
			return builer.ToString();
		}

		public virtual int LineToSrcPos(int line)
		{
			if (mLineData == null)
			{
				return 0;
			}
			// assumes line is added by LineOffset
			line -= mLineOffset;
			return mLineData.GetLineToSrcPos(line);
		}

		public virtual string GetLine(int line)
		{
			if (mLineData == null)
			{
				return NO_SCRIPT;
			}
			// note that this function DOES matter LineOffset
			line -= mLineOffset;
			return mLineData.GetLine(line);
		}

		public virtual bool IsReusable()
		{
			return GetContextCount() == 1 && mTopLevelObject != null;
		}

		public virtual int GetContextCount()
		{
			return mInterCodeObjectList.Count;
		}

		public virtual void Add(InterCodeObject obj)
		{
			mInterCodeObjectList.AddItem(new WeakReference<InterCodeObject>(obj));
		}

		public virtual void Remove(InterCodeObject obj)
		{
			int count = mInterCodeObjectList.Count;
			for (int i = 0; i < count; i++)
			{
				if (mInterCodeObjectList[i].Get() == obj)
				{
					mInterCodeObjectList.Remove(i);
					break;
				}
			}
			CompactInterCodeObjectList();
		}

		public virtual int GetObjectIndex(InterCodeObject obj)
		{
			return mInterCodeObjectList.IndexOf(new WeakReference<InterCodeObject>(obj));
		}

		public virtual InterCodeObject GetCodeObject(int index)
		{
			if (index >= 0 && index < mInterCodeObjectList.Count)
			{
				return mInterCodeObjectList[index].Get();
			}
			else
			{
				return null;
			}
		}
        

		public virtual string GetNameInfo()
		{
			if (mLineOffset == 0)
			{
				return mName;
			}
			else
			{
				return mName + "(line +" + mLineOffset.ToString() + ")";
			}
		}

		public virtual int GetTotalVMCodeSize()
		{
			CompactInterCodeObjectList();
			int size = 0;
			int count = mInterCodeObjectList.Count;
			for (int i = 0; i < count; i++)
			{
				InterCodeObject obj = mInterCodeObjectList[i].Get();
				if (obj != null)
				{
					size += obj.GetCodeSize();
				}
			}
			return size;
		}

		public virtual int GetTotalVMDataSize()
		{
			CompactInterCodeObjectList();
			int size = 0;
			int count = mInterCodeObjectList.Count;
			for (int i = 0; i < count; i++)
			{
				InterCodeObject obj = mInterCodeObjectList[i].Get();
				if (obj != null)
				{
					size += obj.GetDataSize();
				}
			}
			return size;
		}

		public static void ConsoleOutput(string msg, ScriptBlock blk)
		{
			Tjs.OutputToConsole(msg);
		}

		/// <exception cref="VariantException"></exception>
		public virtual void Dump()
		{
			CompactInterCodeObjectList();
			int count = mInterCodeObjectList.Count;
			for (int i = 0; i < count; i++)
			{
				InterCodeObject v = mInterCodeObjectList[i].Get();
				if (v != null)
				{
					ConsoleOutput(string.Empty, this);
					string ptr = $" 0x{v.GetHashCode():X08}";
					ConsoleOutput("(" + v.GetContextTypeName() + ") " + v.GetName() + ptr, this);
					v.Disassemble(this, 0, 0);
				}
			}
		}

		public virtual string GetScript()
		{
			if (mScript != null)
			{
				return mScript;
			}
			else
			{
				return NO_SCRIPT;
			}
		}

		public virtual int CodePosToSrcPos(int codepos)
		{
			return 0;
		}
		// allways 0, 基本的に使われない
	}
}
