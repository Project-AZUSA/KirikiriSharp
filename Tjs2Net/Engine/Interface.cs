/*
 * TJS2 CSharp
 */

namespace Tjs2.Engine
{
	public class Interface
	{
		/// <summary>
		/// メンバーが存在しない时、メンバーを生成する
		/// create a member if not exists
		/// </summary>
		public const int MEMBERENSURE = unchecked((int)(0x00000200));

		/// <summary>メンバーは存在しなければならない member *must* exist ( for Dictionary/Array )</summary>
		public const int MEMBERMUSTEXIST = unchecked((int)(0x00000400));

		/// <summary>プロパティの呼び出しを行わない ignore property invoking</summary>
		public const int IGNOREPROP = unchecked((int)(0x00000800));

		/// <summary>非表示メンバー member is hidden</summary>
		public const int HIDDENMEMBER = unchecked((int)(0x00001000));

		/// <summary>
		/// スタティックメンバー、オブジェクト生成时にコピーされない
		/// member is not registered to the object (internal use)
		/// </summary>
		public const int STATICMEMBER = unchecked((int)(0x00010000));

		/// <summary>
		/// EnumMembers コール时にメンバの实体取得を行わない
		/// values are not retrieved (for EnumMembers)
		/// </summary>
		public const int ENUM_NO_VALUE = unchecked((int)(0x00100000));

		public const int NIS_REGISTER = unchecked((int)(0x00000001));

		public const int NIS_GETINSTANCE = unchecked((int)(0x00000002));

		public const int CII_ADD = unchecked((int)(0x00000001));

		public const int CII_GET = unchecked((int)(0x00000000));

		public const int CII_SET_FINALIZE = unchecked((int)(0x00000002));

		public const int CII_SET_MISSING = unchecked((int)(0x00000003));

		public const int nitMethod = 1;

		public const int nitProperty = 2;
		// set native pointer
		// get native pointer
		// register name
		// 'num' argument passed to CII is to be igonored.
		// retrieve name
		// register "finalize" method name
		// (set empty string not to call the method)
		// 'num' argument passed to CII is to be igonored.
		// register "missing" method name.
		// the method is called when the member is not present.
		// (set empty string not to call the method)
		// 'num' argument passed to CII is to be igonored.
		// the method is to be called with three arguments;
		// get_or_set    : false for get, true for set
		// name          : member name
		// value         : value property; you must
		//               : dereference using unary '*' operator.
		// the method must return true for found, false for not-found.
	}
}
