﻿using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WalkingTec.Mvvm.Core;
using WalkingTec.Mvvm.Core.Extensions;

namespace WalkingTec.Mvvm.TagHelpers.LayUI
{
    /// <summary>
    /// 排序类型
    /// </summary>
    public enum SortTypeEnum
    {
        /// <summary>
        /// 升序
        /// </summary>
        ASC = 0,
        /// <summary>
        /// 降序
        /// </summary>
        DESC
    }
    /// <summary>
    /// 表格风格枚举
    /// </summary>
    public enum DataTableSkinEnum
    {
        /// <summary>
        /// 行边框风格
        /// </summary>
        Line = 0,
        /// <summary>
        /// 列边框风格
        /// </summary>
        Row,
        /// <summary>
        /// 无边框风格
        /// </summary>
        Nob
    }
    /// <summary>
    /// 表格尺寸枚举
    /// </summary>
    public enum DataTableSizeEnum
    {
        /// <summary>
        /// 小尺寸
        /// </summary>
        SM = 0,
        /// <summary>
        /// 大尺寸
        /// </summary>
        LG
    }
    /// <summary>
    /// HTTP Method 
    /// </summary>
    public enum HttpMethodEnum
    {
        /// <summary>
        /// HTTP GET Method
        /// </summary>
        GET = 0,
        /// <summary>
        /// HTTP POST Method
        /// </summary>
        POST
    }
    [HtmlTargetElement("wt:grid", Attributes = REQUIRED_ATTR_NAME, TagStructure = TagStructure.WithoutEndTag)]
    public class DataTableTagHelper : TagHelper
    {
        protected const string REQUIRED_ATTR_NAME = "vm";

        private static readonly string[] SPECIAL_ACTION = new string[] { "Delete", "Edit", "Details" };
        /// <summary>
        /// 用于存储 DataTable render后返回的table变量的前缀
        /// </summary>
        public const string TABLE_JSVAR_PREFIX = "wtVar_";
        /// <summary>
        /// 用于自动生成的 GridId 的前缀
        /// </summary>
        public const string TABLE_ID_PREFIX = "wtTable_";
        /// <summary>
        /// 用于生成操作列
        /// </summary>
        public const string TABLE_TOOLBAR_PREFIX = "wtToolBar_";
        private static JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public ModelExpression Vm { get; set; }

        private IBasePagedListVM<TopBasePoco, BaseSearcher> _listVM;
        private IBasePagedListVM<TopBasePoco, BaseSearcher> ListVM
        {
            get
            {
                if (_listVM == null)
                {
                    _listVM = Vm?.Model as IBasePagedListVM<TopBasePoco, BaseSearcher>;
                }
                return _listVM;
            }
        }

        private string _gridIdUserSet;

        private string _id;
        public string Id
        {
            get
            {
                if (string.IsNullOrEmpty(_id))
                {
                    if (string.IsNullOrEmpty(_gridIdUserSet))
                    {
                        _id = $"{TABLE_ID_PREFIX}{ListVM.UniqueId}";
                    }
                    else
                    {
                        _id = _gridIdUserSet;
                    }
                }
                return _id;
            }
            set
            {
                _id = value;
                _gridIdUserSet = value;
            }
        }

        private string _searchPanelId;
        /// <summary>
        /// 搜索面板 Id
        /// </summary>
        public string SearchPanelId
        {
            get
            {
                if (string.IsNullOrEmpty(_searchPanelId))
                {
                    _searchPanelId = $"{FormTagHelper.FORM_ID_PREFIX}{ListVM.UniqueId}";
                }
                return _searchPanelId;
            }
            set
            {
                _searchPanelId = value;
            }
        }

        private string _tableJSVar;
        /// <summary>
        /// datatable 渲染之后返回对象的变量名
        /// </summary>
        public string TableJSVar
        {
            get
            {
                if (string.IsNullOrEmpty(_tableJSVar))
                {
                    _tableJSVar = $"{TABLE_JSVAR_PREFIX}{(string.IsNullOrEmpty(_gridIdUserSet) ? ListVM.UniqueId : _gridIdUserSet)}";
                }
                return _tableJSVar;
            }
        }

        /// <summary>
        /// 隐藏 Grid 的 Panel 默认false
        /// </summary>
        public bool HiddenPanel { get; set; }
        private string ToolBarId => $"{TABLE_TOOLBAR_PREFIX}{ListVM.UniqueId}";
        /// <summary>
        /// 设定复选框列 默认false
        /// </summary>
        public bool HiddenCheckbox { get; set; }
        /// <summary>
        /// 设定复选框列 默认false
        /// </summary>
        public bool HiddenGridIndex { get; set; }
        /// <summary>
        /// 全部选中
        /// </summary>
        public bool? CheckedAll { get; set; }
        /// <summary>
        /// 复选框在第几列
        /// </summary>
        public int CheckboxIndex { get; set; }

        /// <summary>
        /// 设定容器高度 默认值：'auto' 若height>=0采用 '固定值' 模式，若height 小于 0 采用 'full-差值' 模式
        /// <para>固定值: 设定一个数字，用于定义容器高度，当容器中的内容超出了该高度时，会自动出现纵向滚动条</para>
        /// <para>full-差值: 高度将始终铺满，无论浏览器尺寸如何。这是一个特定的语法格式，其中 full 是固定的，而 差值 则是一个数值，这需要你来预估，比如：表格容器距离浏览器顶部和底部的距离“和” <para>
        /// </summary>
        public int? Height { get; set; }
        /// <summary>
        /// 设定容器宽度 默认值：'auto'
        /// table容器的默认宽度是 auto，你可以借助该参数设置一个固定值，当容器中的内容超出了该宽度时，会自动出现横向滚动条。
        /// </summary>
        public int? Width { get; set; }
        /// <summary>
        /// 接口地址
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// Http Request Method 默认 GET
        /// <para>如果无需自定义HTTP类型，可不加该参数 <see cref="HttpMethodEnum" /></para>
        /// </summary>
        public HttpMethodEnum? Method { get; set; }
        /// <summary>
        /// 查询条件
        /// </summary>
        public Dictionary<string, object> Filter { get; set; }
        /// <summary>
        /// 直接赋值数据
        /// <para>你也可以对表格直接赋值，而无需配置异步数据请求接口。他既适用于只展示一页数据，也非常适用于对一段已知数据进行多页展示。</para>
        /// </summary>
        public ModelExpression Data { get; set; }
        /// <summary>
        /// 直接从 ListVM的EntityList获取数据
        /// </summary>
        public bool UseLocalData { get; set; }
        /// <summary>
        /// 数据渲染完的回调
        /// <para>无论是异步请求数据，还是直接赋值数据，都会触发该回调。你可以利用该回调做一些表格以外元素的渲染。</para>
        /// <para>res:    如果是异步请求数据方式，res即为你接口返回的信息。</para>
        /// <para>        如果是直接赋值的方式，res即为：{data: [], count: 99} data为当前页数据、count为数据总长度</para>
        /// <para>curr:   得到当前页码</para>
        /// <para>count:  得到数据总量</para>
        /// </summary>
        public string DoneFunc { get; set; }
        /// <summary>
        /// 复选框选中事件
        /// 点击复选框时触发，回调函数返回一个object参数，携带的成员如下：
        /// obj.checked //当前是否选中状态
        /// obj.data    //选中行的相关数据
        /// obj.type    //如果触发的是全选，则为：all，如果触发的是单选，则为：one
        /// </summary>
        public string CheckedFunc { get; set; }

        public string PanelTitle { get; set; }

        #region 需要修改
        // TODO 需要修改
        /// <summary>
        /// 初始排序 字段
        /// <para>用于在数据表格渲染完毕时，默认按某个字段排序。注：该参数为 layui 2.1.1 新增</para>
        /// </summary>
        public ModelExpression InitSortField { get; set; }
        /// <summary>
        /// 初始排序 类型默认ASC
        /// <para>用于在数据表格渲染完毕时，默认按某个字段排序。注：该参数为 layui 2.1.1 新增</para>
        /// </summary>
        public SortTypeEnum InitSortType { get; set; }
        #endregion

        /// <summary>
        /// 是否开启分页 默认 true
        /// </summary>
        public bool? Page { get; set; }
        /// <summary>
        /// 每页数据量可选项
        /// <para>默认值：[10,20,30,40,50,70,80,90]</para>
        /// </summary>
        public int[] Limits { get; set; }

        /// <summary>
        /// 默认每页数量 50
        /// </summary>
        public int? Limit { get; set; }
        /// <summary>
        /// 是否显示加载条 默认 true
        /// <para>如果设置 false，则在切换分页时，不会出现加载条。该参数只适用于“异步数据请求”的方式（即设置了url的情况下）</para>
        /// </summary>
        public bool? Loading { get; set; }
        /// <summary>
        /// 用于设定表格风格，若使用默认风格不设置该属性即可
        /// </summary>
        public DataTableSkinEnum? Skin { get; set; }
        /// <summary>
        /// 隔行背景，默认true
        /// <para>若不开启隔行背景，设置为false即可</para>
        /// </summary>
        public bool? Even { get; set; }
        /// <summary>
        /// 用于设定表格尺寸，若使用默认尺寸不设置该属性即可
        /// </summary>
        public DataTableSizeEnum? Size { get; set; }

        /// <summary>
        /// 可编辑
        /// </summary>
        public bool Editable { get; set; }

        /// <summary>
        /// VM
        /// </summary>
        public Type VMType { get; set; }

        /// <summary>
        /// 排除的搜索条件
        /// </summary>
        private static readonly string[] _excludeParams = new string[]
        {
            "Page",
            "Limit",
            "Count",
            "PageCount",
            "FC",
            "DC",
            "VMFullName",
            "SortInfo",
            "TreeMode",
            "IsPostBack",
            "DC",
            "LoginUserInfo"
        };

        /// <summary>
        /// 排除的搜索条件类型，搜索条件数据源可能会存储在Searcher对象中
        /// </summary>
        private static readonly Type[] _excludeTypes = new Type[]
        {
            typeof(List<ComboSelectListItem>),
            typeof(ComboSelectListItem[]),
            typeof(IEnumerable<ComboSelectListItem>)
        };

        private void CalcChildCol(List<List<LayuiColumn>> layuiCols, List<IGridColumn<TopBasePoco>> rawCols, int maxDepth, int depth)
        {
            var tempCols = new List<LayuiColumn>();
            layuiCols.Add(tempCols);

            var nextCols = new List<IGridColumn<TopBasePoco>>();// 下一级列头
            foreach (var item in rawCols)
            {
                var tempCol = new LayuiColumn()
                {
                    Title = item.Title,
                    Field = item.Field,
                    Width = item.Width,
                    Sort = item.Sort,
                    Fixed = item.Fixed,
                    Align = item.Align,
                    UnResize = item.UnResize,
                    //EditType = item.EditType
                };
                if (item.Children != null && item.Children.Count() > 0)
                {
                    tempCol.Colspan = item.ChildrenLength;
                }
                if (maxDepth > 1 && (item.Children == null || item.Children.Count() == 0))
                {
                    tempCol.Rowspan = maxDepth - depth;
                }
                tempCols.Add(tempCol);
                if (item.Children != null && item.Children.Count() > 0)
                    nextCols.AddRange(item.Children);
            }
            if (nextCols.Count > 0)
            {
                CalcChildCol(layuiCols, nextCols, maxDepth, depth + 1);
            }
        }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Loading == null)
            {
                Loading = true;
            }
            var vmQualifiedName = Vm.Model.GetType().AssemblyQualifiedName;
            vmQualifiedName = vmQualifiedName.Substring(0, vmQualifiedName.LastIndexOf(", Version="));

            var tempGridTitleId = Guid.NewGuid().ToNoSplitString();
            output.TagName = "table";
            output.Attributes.Add("id", Id);
            output.Attributes.Add("lay-filter", Id);
            output.TagMode = TagMode.StartTagAndEndTag;
            //IEnumerable<TopBasePoco> data = null;
            if (Limit == null)
            {
                Limit = GlobalServices.GetRequiredService<Configs>()?.RPP;
            }
            if (Limits == null)
            {
                Limits = new int[] { 10, 20, 50, 80, 100, 150, 200 };
                if (!Limits.Contains(Limit.Value))
                {
                    var list = Limits.ToList();
                    list.Add(Limit.Value);
                    Limits = list.OrderBy(x => x).ToArray();
                }
            }
            // TODO 转换有问题
            Page = ListVM.NeedPage;
            if (UseLocalData)
            {
                // 不需要分页
                ListVM.NeedPage = false;
                //data = ListVM.GetEntityList().ToList();
            }
            else if (string.IsNullOrEmpty(Url))
            {
                Url = "/_Framework/GetPagingData";

                if (Filter == null) Filter = new Dictionary<string, object>();
                Filter.Add("_DONOT_USE_VMNAME", vmQualifiedName);
                Filter.Add("SearcherMode", ListVM.SearcherMode);
                if (ListVM.Ids != null && ListVM.Ids.Count > 0)
                {
                    Filter.Add("Ids", ListVM.Ids);
                }
                // 为首次加载添加Searcher查询参数
                if (ListVM.Searcher != null)
                {
                    var props = ListVM.Searcher.GetType().GetProperties();
                    props = props.Where(x => !_excludeTypes.Contains(x.PropertyType)).ToArray();
                    foreach (var prop in props)
                    {
                        if (!_excludeParams.Contains(prop.Name))
                        {
                            Filter.Add($"Searcher.{prop.Name}", prop.GetValue(ListVM.Searcher));
                        }
                    }
                }
            }

            var request = new Dictionary<string, object>
            {
                {"pageName","Searcher.Page" },   //页码的参数名称，默认：page
                {"limitName","Searcher.Limit" }, //每页数据量的参数名，默认：limit
            };
            var response = new Dictionary<string, object>
            {
                {"statusName","Code" }, //数据状态的字段名称，默认：code
                {"statusCode",200 },    //成功的状态码，默认：0
                {"msgName","Msg" },     //状态信息的字段名称，默认：msg
                {"countName","Count" }, //数据总数的字段名称，默认：count
                {"dataName","Data" }    //数据列表的字段名称，默认：data
            };

            #region 生成 Layui 所需的表头
            var rawCols = ListVM?.GetHeaders();
            var maxDepth = (ListVM?.ChildrenDepth) ?? 1;
            var layuiCols = new List<List<LayuiColumn>>();
            var tempCols = new List<LayuiColumn>();
            layuiCols.Add(tempCols);
            // 添加复选框
            if (!HiddenCheckbox)
            {
                var checkboxHeader = new LayuiColumn()
                {
                    Type = LayuiColumnTypeEnum.Checkbox,
                    LAY_CHECKED = CheckedAll,
                    Rowspan = maxDepth
                };
                tempCols.Add(checkboxHeader);
            }
            // 添加序号列
            if (!HiddenGridIndex)
            {
                var gridIndex = new LayuiColumn()
                {
                    Type = LayuiColumnTypeEnum.Numbers,
                    Rowspan = maxDepth
                };
                tempCols.Add(gridIndex);
            }
            var nextCols = new List<IGridColumn<TopBasePoco>>();// 下一级列头
            foreach (var item in rawCols)
            {
                var tempCol = new LayuiColumn()
                {
                    Title = item.Title,
                    Field = item.Field,
                    Width = item.Width,
                    Sort = item.Sort,
                    Fixed = item.Fixed,
                    Align = item.Align,
                    UnResize = item.UnResize,
                    //EditType = item.EditType
                };
                switch (item.ColumnType)
                {
                    case GridColumnTypeEnum.Space:
                        tempCol.Type = LayuiColumnTypeEnum.Space;
                        break;
                    case GridColumnTypeEnum.Action:
                        tempCol.Toolbar = $"#{ToolBarId}";
                        break;
                    default:
                        break;
                }
                if (item.Children != null && item.Children.Count() > 0)
                {
                    tempCol.Colspan = item.ChildrenLength;
                }
                if (maxDepth > 1 && (item.Children == null || item.Children.Count() == 0))
                {
                    tempCol.Rowspan = maxDepth;
                }
                tempCols.Add(tempCol);
                if (item.Children != null && item.Children.Count() > 0)
                    nextCols.AddRange(item.Children);
            }
            if (nextCols.Count > 0)
            {
                CalcChildCol(layuiCols, nextCols, maxDepth, 1);
            }

            #endregion

            #region 处理 DataTable 操作按钮

            var actionCol = ListVM?.GridActions;

            var rowBtnStrBuilder = new StringBuilder();// Grid 行内按钮
            var toolBarBtnStrBuilder = new StringBuilder();// Grid 工具条按钮
            var gridBtnEventStrBuilder = new StringBuilder();// Grid 按钮事件

            if (actionCol != null && actionCol.Count > 0)
            {
                var vm = Vm.Model as BaseVM;
                foreach (var item in actionCol)
                {
                    if (vm.LoginUserInfo?.IsAccessable(item.Url) == true || item.ParameterType == GridActionParameterTypesEnum.AddRow || item.ParameterType == GridActionParameterTypesEnum.RemoveRow)
                    {
                        // Grid 行内按钮
                        if (item.ShowInRow)
                        {
                            if (item.ParameterType != GridActionParameterTypesEnum.RemoveRow)
                            {
                                rowBtnStrBuilder.Append($@"<a class=""layui-btn layui-btn-primary layui-btn-xs"" lay-event=""{item.Area + item.ControllerName + item.ActionName + item.QueryString}"">{item.Name}</a>");
                            }
                            else
                            {
                                rowBtnStrBuilder.Append($@"<a class=""layui-btn layui-btn-primary layui-btn-xs"" onclick=""ff.RemoveGridRow('{Id}',{Id}option,{{{{d.LAY_INDEX}}}});"">{item.Name}</a>");
                            }

                        }

                        // Grid 工具条按钮
                        if (!item.HideOnToolBar)
                        {
                            var icon = string.Empty;
                            switch (item.ActionName)
                            {
                                case "Create":
                                    icon = @"<i class=""layui-icon"">&#xe654;</i>";
                                    break;
                                case "Delete":
                                case "BatchDelete":
                                    icon = @"<i class=""layui-icon"">&#xe640;</i>";
                                    break;
                                case "Edit":
                                case "BatchEdit":
                                    icon = @"<i class=""layui-icon"">&#xe642;</i>";
                                    break;
                                case "Details":
                                    icon = @"<i class=""layui-icon"">&#xe60e;</i>";
                                    break;
                                case "Import":
                                    icon = @"<i class=""layui-icon"">&#xe630;</i>";
                                    break;
                                case "GetExportExcel":
                                    icon = @"<i class=""layui-icon"">&#xe62d;</i>";
                                    break;
                            }
                            toolBarBtnStrBuilder.Append($@"<a href=""javascript:void(0)"" onclick=""wtToolBarFunc_{Id}({{event:'{item.Area + item.ControllerName + item.ActionName + item.QueryString}'}});"" class=""layui-btn layui-btn-sm"">{icon}{item.Name}</a>");
                        }
                        var url = item.Url;
                        if (item.ControllerName == "_Framework" && item.ActionName == "GetExportExcel") // 导出按钮 接口地址
                        {
                            url = $"{url}&_DONOT_USE_VMNAME={vmQualifiedName}";
                        }
                        var script = new StringBuilder($"var tempUrl = '{url}';");
                        if (SPECIAL_ACTION.Contains(item.ActionName))
                        {
                            script.Append($@"tempUrl = tempUrl + '&id=' + data.ID;");
                        }
                        else
                        {
                            switch (item.ParameterType)
                            {
                                case GridActionParameterTypesEnum.NoId: break;
                                case GridActionParameterTypesEnum.SingleId:
                                    script.Append($@"
if(data==undefined||data==null||data.ID==undefined||data.ID==null){{
    var ids = ff.GetSelections('{Id}');
    if(ids.length == 0){{
        layui.layer.msg('请选择一行');
        return;
    }}else if(ids.length > 1){{
        layui.layer.msg('最多只能选择一行');
        return;
    }}else{{
        tempUrl = tempUrl + '&id=' + ids[0];
    }}
}}else{{
    tempUrl = tempUrl + '&id=' + data.ID;
}}
");
                                    break;
                                case GridActionParameterTypesEnum.MultiIds:
                                    script.Append($@"
isPost = true;
var ids = ff.GetSelections('{Id}');
if(ids.length == 0){{
    layui.layer.msg('请至少选择一行');
    return;
}}
");
                                    break;
                                case GridActionParameterTypesEnum.SingleIdWithNull:
                                    script.Append($@"
var ids = [];
if(data != null && data.ID != null){{
    ids.push(data.ID);
}} else {{
    ids = ff.GetSelections('{Id}');
}}
if(ids.length > 1){{
    layui.layer.msg('最多只能选择一行');
    return;
}}else if(ids.length == 1){{
    tempUrl = tempUrl + '&id=' + ids[0];
}}
");
                                    break;
                                case GridActionParameterTypesEnum.MultiIdWithNull:
                                    script.Append($@"
var ids = ff.GetSelections('{Id}');
{(item.ControllerName == "_Framework" && item.ActionName == "GetExportExcel" ? "if(ids.length>0) tempUrl = tempUrl + '&Ids=' + ids.join('&Ids=');" : "isPost = true;")}
");
                                    break;
                                default: break;
                            }
                        }

                        gridBtnEventStrBuilder.Append($@"
case '{item.Area + item.ControllerName + item.ActionName + item.QueryString}':{{");
                        if (item.ParameterType == GridActionParameterTypesEnum.AddRow)
                        {
                            gridBtnEventStrBuilder.Append($@"ff.AddGridRow(""{Id}"",{Id}option,{ListVM.GetSingleDataJson(null)});
");
                        }
                        else if (item.ParameterType == GridActionParameterTypesEnum.RemoveRow)
                        {

                        }
                        else
                        {
                            gridBtnEventStrBuilder.Append($@"
var isPost = false;
{script}
{(string.IsNullOrEmpty(item.OnClickFunc) ?
        (item.ShowDialog ?
            $"ff.OpenDialog(tempUrl,'{Guid.NewGuid().ToNoSplitString()}','{item.DialogTitle}',{(item.DialogWidth == null ? "null" : item.DialogWidth.ToString())},{(item.DialogHeight == null ? "null" : item.DialogHeight.ToString())},isPost===true&&ids!==null&&ids!==undefined?{{'Ids':ids}}:undefined);"
            : (item.Area == string.Empty && item.ControllerName == "_Framework" && item.ActionName == "GetExportExcel" ?
                $"ff.DownloadExcelOrPdf(tempUrl,'{SearchPanelId}',{JsonConvert.SerializeObject(Filter)});"
                : $"ff.BgRequest(tempUrl, isPost===true&&ids!==null&&ids!==undefined?{{'Ids':ids}}:undefined);"
                )
        )
        : $"{item.OnClickFunc}();")}");
                        }
                        gridBtnEventStrBuilder.Append($@"}};break;
");
                    }
                }
            }
            #endregion

            #region DataTable

            var vmName = string.Empty;
            if (VMType != null)
            {
                var vmQualifiedName1 = VMType.AssemblyQualifiedName;
                vmName = vmQualifiedName1.Substring(0, vmQualifiedName1.LastIndexOf(", Version="));
            }
            output.PostElement.AppendHtml($@"
<script>
  var table = layui.table;
  /* 暂时解决 layui table首次及table.reload()无loading的bug */
  var layer = layui.layer;
  var msg = layer.msg('数据请求中', {{
    icon: 16,
    time: -1,
    anim: -1,
    fixed: false
  }})
  /* 暂时解决 layui table首次及table.reload()无loading的bug */
var {Id}option = {{
    elem: '#{Id}'
    ,id: '{Id}'
    {(string.IsNullOrEmpty(Url) ? string.Empty : $",url: '{Url}'")}
    {(Filter == null || Filter.Count == 0 ? string.Empty : $",where: {JsonConvert.SerializeObject(Filter)}")}
    {(Method == null ? ",method:'post'" : $",method: '{Method.Value.ToString().ToLower()}'")}
    {(UseLocalData ? $",data: {ListVM.GetDataJson()}" : string.Empty)}
    {(!Loading.HasValue ? string.Empty : $",loading: {Loading.Value.ToString().ToLower()}")}
    ,request: {JsonConvert.SerializeObject(request)}
    ,response: {JsonConvert.SerializeObject(response)}
    {(Page ?? true ? ",page:true" : ",page:{layout:['count']}")}
    ,limit: {(Page ?? true ? $"{Limit ?? 50}" : $"{(UseLocalData ? ListVM.GetEntityList().Count().ToString() : "0")}")}
    {(Page ?? true ?
        (Limits == null || Limits.Length == 0 ? ",limits:[10,20,50,80,100,150,200]" : $",limits:{JsonConvert.SerializeObject(Limits)}")
        : string.Empty)}
    {(!Width.HasValue ? string.Empty : $",width: {Width.Value}")}
    {(!Height.HasValue ? string.Empty : (Height.Value >= 0 ? $",height: {Height.Value}" : $",height: 'full{Height.Value}'"))}
    {(InitSortField == null ? string.Empty : $",initSort: {{field: '{InitSortField.Metadata.PropertyName}',type: '{InitSortType.ToString().ToLower()}'}}")}
    ,cols:{JsonConvert.SerializeObject(layuiCols, _jsonSerializerSettings)}
    {(!Skin.HasValue ? string.Empty : $",skin: '{Skin.Value.ToString().ToLower()}'")}
    {(!Even.HasValue ? ",even: true" : $",even: {Even.Value.ToString().ToLower()}")}
    {(!Size.HasValue ? string.Empty : $",size: '{Size.Value.ToString().ToLower()}'")}
,done: function(res,curr,count){{layer.close(msg);
    {(string.IsNullOrEmpty(DoneFunc) ? string.Empty : $"{DoneFunc}(res,curr,count)")}
}}
}}


  {TableJSVar} = table.render({Id}option);
  
  // 监听工具条
  function wtToolBarFunc_{Id}(obj){{ //注：tool是工具条事件名，test是table原始容器的属性 lay-filter=""对应的值""
    var data = obj.data, layEvent = obj.event, tr = obj.tr; //获得当前行 tr 的DOM对象
    {(gridBtnEventStrBuilder.Length == 0 ? string.Empty : $@"switch(layEvent){{{gridBtnEventStrBuilder}default:break;}}")}
    return;
  }}
  {(VMType == null || string.IsNullOrEmpty(vmName) ? string.Empty : $@"function wtEditFunc_{Id}(o){{
      var data = {{_DONOT_USE_VMNAME:'{vmName}',id:o.data.ID,field:o.field,value:o.value}};
      $.post(""/_Framework/UpdateModelProperty"",data,function(a,b,c){{
          if(a.code == 200){{ff.Msg('更新成功');}}else{{ff.Msg(a.msg);}}
      }});
  }}")}
  table.on('tool({Id})',wtToolBarFunc_{Id});
  {(VMType == null || string.IsNullOrEmpty(vmName) ? string.Empty : $"table.on('edit({Id})',wtEditFunc_{Id});")}
  {(string.IsNullOrEmpty(CheckedFunc) ? string.Empty : $"table.on('checkbox({Id})',{CheckedFunc});")}
</script>
<!-- Grid 行内按钮 -->
<script type=""text/html"" id=""{ToolBarId}"">{rowBtnStrBuilder}</script>
");
            #endregion

            if (HiddenPanel) // 无 Panel
            {
                output.PreElement.AppendHtml($@"<div style=""text-align:right"">{toolBarBtnStrBuilder}</div>");
            }
            else // 有 Panel
            {
                #region 在数据列表外部套上一层 Panel

                output.PreElement.AppendHtml($@"
<div class=""layui-collapse"" >
  <div class=""layui-colla-item"">
    <h2 id=""{tempGridTitleId}"" class=""layui-colla-title"">{PanelTitle ?? "数据列表"}
      <!-- 数据列表按钮组 -->
      <div style=""text-align:right;margin-top:-43px;"">{toolBarBtnStrBuilder}</div>
    </h2>
    <div class=""layui-colla-content layui-show"" style=""padding:0;"">
");
                output.PostElement.AppendHtml($@"
    </div>
  </div>
</div>
<script>layui.element.init();/*阻止事件冒泡*/$('#{tempGridTitleId} .layui-btn').on('click',function(e){{e.stopPropagation();}})</script>");

                #endregion
            }
            output.PostElement.AppendHtml($@"
{ (string.IsNullOrEmpty(ListVM.DetailGridPrix) ? "" : $"<input type=\"hidden\" name=\"{Vm.Name}.DetailGridPrix\" value=\"{ListVM.DetailGridPrix}\"/>")}
");
            base.Process(context, output);
        }
    }
}
