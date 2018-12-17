/*
 * TJS2 CSharp
 */

namespace Tjs2.Engine
{
	public interface SourceCodeAccessor
	{
		/// <summary>バイトコード位置からソースコード位置を得る</summary>
		/// <param name="codepos">バイトコード位置</param>
		/// <returns>ソースコード位置</returns>
		int CodePosToSrcPos(int codepos);

		/// <summary>ソースコード位置から行数を得る</summary>
		/// <param name="srcpos">ソースコード位置</param>
		/// <returns>行数</returns>
		int SrcPosToLine(int srcpos);

		/// <summary>指定ラインの文字列を得る</summary>
		/// <param name="line">行数</param>
		/// <returns>指定行の文字列</returns>
		string GetLine(int line);

		/// <summary>スクリプトを全文取得する</summary>
		/// <returns>スクリプト文</returns>
		string GetScript();

		/// <summary>スクリプト开始オフセットを取得する</summary>
		/// <returns>オフセット</returns>
		int GetLineOffset();

		/// <summary>スクリプト名を取得する</summary>
		/// <returns>スクリプト名</returns>
		string GetName();
	}
}
