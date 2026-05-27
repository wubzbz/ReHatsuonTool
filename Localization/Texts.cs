namespace ReHatsuonTool.Localization;

/// <summary>
/// Centralized UI text resources. Edit strings here instead of hunting through code.
/// </summary>
public static class Texts
{
    // Navigation
    public const string UnknownVer = "未知版本";
    public const string LicenseInfo = "MIT License © 2026 by wubzbz";

    // Window / Page titles
    public const string WindowTitle = "Re-Hatsuon Tool - 添加普通话语音物件";
    public const string NavSetup = "初始设定";
    public const string NavAdd = "添加台词";
    public const string NavSave = "保存到...";

    // Setup page
    public const string SetupPageTitle = "初始设定";
    public const string SetupCompleteBanner = "初始设定已完成，可以开始编辑台词啦！";
    public const string Ymm4StepTitle = "1. 选择 YMM4 安装目录";
    public const string Ymm4StepExeLabel = "EXE: ";
    public const string Ymm4StepHint = "path/to/YukkuriMovieMaker/";
    public const string Ymm4StepWarning = "请选择YMM4所在的目录 (含有 YukkuriMovieMaker.exe)";
    public const string YmmpStepTitle = "2. 选择 ymmp 项目";
    public const string YmmpStepHint = "YourProject.ymmp";
    public const string YmmpStepWarning = "请选择有效的 ymmp 项目 (Timelines.Items 不能为空)";

    // AddLines page
    public const string AddLinesPageTitle = "添加台词";
    public const string AddLinesTimelineLabel = "时间线: ";
    public const string AddLinesTimelineHint = "选择目标时间线";
    public const string AddLinesImportCsvButton = "从 CSV 导入";
    public const string AddLinesCharacterHint = "角色";
    public const string AddLinesSerifHint = "台词文本";
    public const string AddLinesDeleteTooltip = "删除此行";
    public const string AddLinesDeleteSymbol = "❌";

    // Save page
    public const string SavePageTitle = "保存到...";
    public const string SaveExportSectionTitle = "导出为 CSV 台本";
    public const string SaveYmmpAppendSectionTitle = "追加到 ymmp 项目";
    public const string SaveExportButton = "导出到...";
    public const string SaveExportDesc1 = "将保存 ";
    public const string SaveExportDesc2 = " 条台词为 CSV 台本文件。编码格式为 UTF-8 BOM。";
    public const string SaveExportedTo = "已保存到 ";
    public const string SaveAppendButton = "追加到 ymmp";
    public const string SaveKeepLines = "在追加后保留台词";
    public const string SaveYmmpPathLabel = "项目路径:";
    public const string SaveYmmpTimelineLabel = " 时 间 线:";
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
    public const string SaveCsvDefaultExt = "csv";
    public const string SaveCsvDefaultFileName = "script.csv";
    public const string SaveImportCsvTitle = "import CSV";
    public const string SaveImportCsvFilter = "CSV file (*.csv)|*.csv|All files (*.*)|*.*";
    public const string SaveYmmpItemsCountUnit = " 个";
    public const string SaveYmmpStartTimeFormat = "{0:D2}:{1:D2}:{2:D2}.{3:D2}";
    public const string SaveMessageBoxErrorTitle = "Error";

    // Ymmp appender error messages
    public const string YmmpAppendFileNotFound = "ymmp file not found";
    public const string YmmpAppendNoValidLines = "no valid lines";
    public const string YmmpAppendJsonParseFailed = "JSON parse failed";
    public const string YmmpAppendNoTimelinesFound = "no Timelines found";
    public const string YmmpAppendTimelineNotFound = "timeline not found";

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

    // File dialogs
    public const string DlgYmm4Title = "选择 YMM4 安装目录";
    public const string DlgYmmpTitle = "选择 ymmp 项目文件";
    public const string DlgYmmpFilter = "YMM4 项目 (*.ymmp)|*.ymmp|所有文件 (*.*)|*.*";

    // Python diagnostics
    public const string PythonDllNotFound = "Python DLL not found. Searched: {0}";
    public const string PythonNetOk = "Python.NET OK. {0}";
    public const string PythonNetError = "Python.NET error: {0}";

    // Update checking
    public const string UpdateAvailable = "✨点击更新 ← {0}";
}
