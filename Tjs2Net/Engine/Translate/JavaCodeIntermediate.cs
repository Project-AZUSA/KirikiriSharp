/*
 * TJS2 CSharp
 */

using System.Collections.Generic;
using Tjs2.Sharpen;

namespace Tjs2.Engine.Translate
{
	public class JavaCodeIntermediate
	{
		public class ClosureCode
		{
			public AList<string> mCode;

			public int mType;

			public string mName;

			public ClosureCode(string name, int type, AList<string> code)
			{
				// context type
				mName = name;
				mType = type;
				mCode = code;
			}

			/// <exception cref="TjsException"></exception>
			public virtual void Write(string classname, TextWriteStreamInterface stream)
			{
				if (mType == ContextType.FUNCTION)
				{
					if (mName.Equals(classname))
					{
						stream.Write("registerNCM( \"" + mName + "\", new NativeConvertedClassConstructor(engine) {\n@Override\n"
							);
					}
					else
					{
						stream.Write("registerNCM( \"" + mName + "\", new NativeConvertedClassMethod(engine) {\n@Override\n"
							);
					}
				}
				AList<string> ca = mCode;
				int count = ca.Count;
				for (int i = 0; i < count; i++)
				{
					string line = ca[i];
					if (line != null)
					{
						stream.Write(ca[i]);
						stream.Write("\n");
					}
				}
				if (mType == ContextType.FUNCTION)
				{
					stream.Write("}, CLASS_NAME, Interface.nitMethod, 0 );\n\n");
				}
			}
		}

		public class Property
		{
			public AList<string> mSetter;

			public AList<string> mGetter;

			public string mName;

			public Property(string name)
			{
				mName = name;
			}

			public virtual void SetGetter(AList<string> code)
			{
				mGetter = code;
			}

			public virtual void SetSetter(AList<string> code)
			{
				mSetter = code;
			}
		}

		private AList<string> mInitializer;

		private AList<JavaCodeIntermediate.ClosureCode> mMembers;

		private Dictionary<string, JavaCodeIntermediate.Property> mProps;

		private string mName;

		public JavaCodeIntermediate(string classname)
		{
			mName = classname;
			mMembers = new AList<JavaCodeIntermediate.ClosureCode>();
			mProps = new Dictionary<string, JavaCodeIntermediate.Property>();
		}

		public virtual void AddMember(JavaCodeIntermediate.ClosureCode code)
		{
			mMembers.AddItem(code);
		}

		public virtual void AddProperty(string name, JavaCodeIntermediate.Property prop)
		{
			mProps.Put(name, prop);
		}

		public virtual JavaCodeIntermediate.Property GetProperty(string name)
		{
			return mProps.Get(name);
		}

		public virtual void SetInitializer(AList<string> code)
		{
			mInitializer = code;
		}

		public virtual string GetName()
		{
			return mName;
		}

		/// <exception cref="TjsException"></exception>
		public virtual void Write()
		{
			string classname = mName + "Class";
			string filename = mName + "Class.java";
			TextWriteStreamInterface stream = Tjs.mStorage.CreateTextWriteStream(filename, "utf-8"
				);
			stream.Write("package jp.kirikiri.tjs2java;\n");
			stream.Write("import jp.kirikiri.tjs2.*;\n");
			stream.Write("import jp.kirikiri.tjs2.Error;\n");
			stream.Write("import jp.kirikiri.tvp2.base.ScriptsClass;\n");
			stream.Write("import jp.kirikiri.tvp2.msg.Message;\n");
			stream.Write("public class " + classname + " extends ExtendableNativeClass {\n");
			stream.Write("static public int ClassID = -1;\n");
			stream.Write("static public final String CLASS_NAME = \"" + mName + "\";\n");
			stream.Write("public " + classname + "() throws VariantException, TJSException {\n"
				);
			stream.Write("super( CLASS_NAME );\n");
			stream.Write("final int NCM_CLASSID = TJS.registerNativeClass(CLASS_NAME);\n");
			stream.Write("setClassID( NCM_CLASSID );\n");
			stream.Write("ClassID = NCM_CLASSID;\n");
			stream.Write("TJS engine = ScriptsClass.getEngine();\n");
			int count = mMembers.Count;
			for (int i = 0; i < count; i++)
			{
				mMembers[i].Write(mName, stream);
			}
			stream.Write("}\n}\n");
			stream.Destruct();
		}
	}
}
