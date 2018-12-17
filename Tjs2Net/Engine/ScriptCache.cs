/*
 * TJS2 CSharp
 */

using System.Collections.Generic;
using Tjs2.Sharpen;

namespace Tjs2.Engine
{
	public class ScriptCache
	{
		private const int SCRIPT_CACHE_MAX = 64;

		internal class ScriptCacheData
		{
			public string mScript;

			public bool mExpressionMode;

			public bool mMustReturnResult;

			public override bool Equals(object o)
			{
				if (o is ScriptCache.ScriptCacheData && o != null)
				{
					ScriptCache.ScriptCacheData rhs = (ScriptCache.ScriptCacheData)o;
					return (mScript.Equals(rhs.mScript) && mExpressionMode == rhs.mExpressionMode && 
						mMustReturnResult == rhs.mMustReturnResult);
				}
				else
				{
					return false;
				}
			}

			public override int GetHashCode()
			{
				int v = mScript.GetHashCode();
				v ^= mExpressionMode ? 1 : 0;
				v ^= mMustReturnResult ? 1 : 0;
				return v;
			}
		}

		private Tjs mOwner;

		private Dictionary<ScriptCache.ScriptCacheData, ScriptBlock> mCache;

		public ScriptCache(Tjs owner)
		{
			mOwner = owner;
			mCache = new Dictionary<ScriptCache.ScriptCacheData, ScriptBlock>(SCRIPT_CACHE_MAX
				);
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		/// <exception cref="CompileException"></exception>
		public virtual void ExecScript(string script, Variant result, Dispatch2 context, 
			string name, int lineofs)
		{
			Compiler compiler = new Compiler(mOwner);
			if (name != null)
			{
				compiler.SetName(name, lineofs);
			}
			ScriptBlock blk = compiler.DoCompile(script, false, result != null);
			compiler = null;
			blk.ExecuteTopLevel(result, context);
			if (blk.GetContextCount() == 0)
			{
				mOwner.RemoveScriptBlock(blk);
			}
			blk.Compact();
			blk = null;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		/// <exception cref="CompileException"></exception>
		public virtual void EvalExpression(string expression, Variant result, Dispatch2 context
			, string name, int lineofs)
		{
			// currently this works only with anonymous script blocks.
			// note that this function is basically the same as function above.
			if (name != null && name.Length > 0)
			{
				Compiler compiler = new Compiler(mOwner);
				compiler.SetName(name, lineofs);
				ScriptBlock blk = compiler.DoCompile(expression, true, result != null);
				compiler = null;
				if (blk != null)
				{
					blk.ExecuteTopLevel(result, context);
					if (blk.GetContextCount() == 0)
					{
						mOwner.RemoveScriptBlock(blk);
					}
					blk.Compact();
					blk = null;
				}
				return;
			}
			// search through script block cache
			ScriptCache.ScriptCacheData data = new ScriptCache.ScriptCacheData();
			data.mScript = expression;
			data.mExpressionMode = true;
			data.mMustReturnResult = result != null;
			ScriptBlock block = mCache.Get(data);
			if (block != null)
			{
				// found in cache
				// execute script block in cache
				block.ExecuteTopLevelScript(result, context);
				return;
			}
			// not found in cache
			Compiler compiler_1 = new Compiler(mOwner);
			compiler_1.SetName(name, lineofs);
			ScriptBlock blk_1 = compiler_1.DoCompile(expression, true, result != null);
			blk_1.ExecuteTopLevel(result, context);
			bool preprocess = compiler_1.IsUsingPreProcessor();
			compiler_1 = null;
			//ScriptBlock blk = new ScriptBlock(mOwner);
			//blk.setText( result, expression, context, true);
			// add to cache
			if (blk_1.IsReusable() && !preprocess)
			{
				// currently only single-context script block is cached
				mCache.Put(data, blk_1);
			}
			else
			{
				if (blk_1.GetContextCount() == 0)
				{
					mOwner.RemoveScriptBlock(blk_1);
				}
			}
			blk_1.Compact();
			blk_1 = null;
			return;
		}
	}
}
