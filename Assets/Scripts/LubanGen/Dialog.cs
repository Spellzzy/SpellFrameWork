
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Luban;
using SimpleJSON;


namespace cfg
{
public sealed partial class Dialog : Luban.BeanBase
{
    public Dialog(JSONNode _buf) 
    {
        { if(!_buf["id"].IsNumber) { throw new SerializationException(); }  Id = _buf["id"]; }
        { var __json0 = _buf["content"]; if(!__json0.IsArray) { throw new SerializationException(); } int _n0 = __json0.Count; Content = new int[_n0]; int __index0=0; foreach(JSONNode __e0 in __json0.Children) { int __v0;  { if(!__e0.IsNumber) { throw new SerializationException(); }  __v0 = __e0; }  Content[__index0++] = __v0; }   }
    }

    public static Dialog DeserializeDialog(JSONNode _buf)
    {
        return new Dialog(_buf);
    }

    /// <summary>
    /// 对话组ID
    /// </summary>
    public readonly int Id;
    /// <summary>
    /// 包含的对话信息
    /// </summary>
    public readonly int[] Content;
   
    public const int __ID__ = 2046749032;
    public override int GetTypeId() => __ID__;

    public  void ResolveRef(Tables tables)
    {
        
        
    }

    public override string ToString()
    {
        return "{ "
        + "id:" + Id + ","
        + "content:" + Luban.StringUtil.CollectionToString(Content) + ","
        + "}";
    }
}

}
