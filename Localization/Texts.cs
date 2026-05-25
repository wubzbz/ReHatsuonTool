namespace ReHatsuonTool.Localization;

/// <summary>
/// Centralized UI text resources. Edit strings here instead of hunting through code.
/// </summary>
public static class Texts
{
    // Ymm4 detection
    public const string DetectDirEmpty = "请输入 YMM4 安装目录";
    public const string DetectDirNotFound = "目录不存在";
    public const string DetectExeNotFound = "未找到 YukkuriMovieMaker.exe";
    public const string DetectNoUserSetting = "找到 exe，但 user\\setting 目录不存在（可能尚未运行过 YMM4）";
    public const string DetectNoVersionFolder = "user\\setting 下未找到版本号文件夹";
    public const string DetectNoCharSettings = "找到版本 {0}，但角色设定文件不存在";
    public const string DetectVersionFormat = "YMM4 版本: {0}";
    public const string DetectVersionFailed = "无法检测版本";

    // Ymmp validation
    public const string YmmpFileNotFound = "文件不存在";
    public const string YmmpNoTimelines = "缺少 Timelines 字段";
    public const string YmmpTimelinesEmpty = "Timelines 为空";
    public const string YmmpTimelineMissingItems = "时间线 \"{0}\" 缺少 Items 字段";
    public const string YmmpItemsNotArray = "时间线 \"{0}\" 的 Items 不是数组";
    public const string YmmpJsonInvalid = "JSON 格式无效: {0}";
    public const string YmmpReadFailed = "读取失败: {0}";
    public const string YmmpSummaryFormat = "  Timelines: {0}, VoiceItems: {1}";
    public const string YmmpTimelineItemFormat = "{0} ({1} 个语音物件)";
    public const string YmmpErrorPrefix = "【错误】";

    // Character settings
    public const string CharsEmpty = "0 个角色";
    public const string CharsSummaryFormat = "  共 {0} 个角色:";
    public const string CharsMoreFormat = "    ... 还有 {0} 个";
    public const string CharsUnknownName = "?";

    // Save page
    public const string SavePageTitle = "保存到...";
    public const string SaveExportSectionTitle = "导出为 CSV 台本";
    public const string SaveYmmpAppendSectionTitle = "追加到 ymmp 项目";
    public const string SaveExportButton = "导出到...";
    public const string SaveExportDesc = "将保存 {0} 条台词为 CSV 台本文件。编码格式为 UTF-8 BOM。";
    public const string SaveExportedTo = "已保存到 {0}";
    public const string SaveAppendButton = "追加到 ymmp";
    public const string SaveKeepLines = "在追加后保留台词";
    public const string SaveYmmpPathLabel = "路    径:";
    public const string SaveYmmpTimelineLabel = "时 间 线:";
    public const string SaveYmmpItemCountLabel = "已有物件:";
    public const string SaveYmmpStartTimeLabel = "起始时点:";
    public const string SaveYmmpNotSet = "（未设置）";
    public const string SaveYmmpValidationFailed = "验证失败";
    public const string SaveAppendSuccess = "已在 {0} 追加 {1} 个语音物件到时间线 \"{2}\"";
    public const string SaveYmmpNotFound = "ymmp 文件不存在";
    public const string SavePythonUnavailable = "Python 不可用。请检查 python 文件夹位置。";
    public const string SaveConversionFallback = "⚠ 发音转换未生效，已回退到原文。";
    public const string SaveYmmpAppendFailed = "追加失败: {0}";
    public const string SaveCsvExportFailed = "CSV export failed: {0}";
    public const string SaveCsvDialogTitle = "导出 CSV 台本";
    public const string SaveCsvDialogFilter = "CSV file (*.csv)|*.csv";
    public const string SaveImportCsvTitle = "import CSV";
    public const string SaveImportCsvFilter = "CSV file (*.csv)|*.csv|All files (*.*)|*.*";
    public const string SaveYmmpItemsCountUnit = " 个";
    public const string SaveYmmpStartTimeFormat = "{0:D2}:{1:D2}:{2:D2}:{3:D2}";

    // File dialogs
    public const string DlgYmm4Title = "选择 YMM4 安装目录";
    public const string DlgYmmpTitle = "选择 ymmp 项目文件";
    public const string DlgYmmpFilter = "YMM4 项目 (*.ymmp)|*.ymmp|所有文件 (*.*)|*.*";
}
