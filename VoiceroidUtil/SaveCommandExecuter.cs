﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using RucheHome.AviUtl.ExEdit;
using RucheHome.Util;
using RucheHome.Voiceroid;
using RucheHome.Windows.Mvvm.Commands;

namespace VoiceroidUtil
{
    /// <summary>
    /// ReactiveCommand でWAVEファイル保存処理の非同期実行を行うためのクラス。
    /// </summary>
    public class SaveCommandExecuter : AsyncCommandExecuter<SaveCommandExecuter.Parameter>
    {
        /// <summary>
        /// コマンドパラメータクラス。
        /// </summary>
        public class Parameter
        {
            /// <summary>
            /// コンストラクタ。
            /// </summary>
            /// <param name="process">VOICEROIDプロセス。</param>
            /// <param name="talkTextReplaceConfig">トークテキスト置換設定。</param>
            /// <param name="exoConfig">AviUtl拡張編集ファイル用設定。</param>
            /// <param name="appConfig">アプリ設定。</param>
            /// <param name="talkText">トークテキスト。</param>
            public Parameter(
                IProcess process,
                TalkTextReplaceConfig talkTextReplaceConfig,
                ExoConfig exoConfig,
                AppConfig appConfig,
                string talkText)
            {
                this.Process = process;
                this.TalkTextReplaceConfig = talkTextReplaceConfig;
                this.ExoConfig = exoConfig;
                this.AppConfig = appConfig;
                this.TalkText = talkText;
            }

            /// <summary>
            /// VOICEROIDプロセスを取得する。
            /// </summary>
            public IProcess Process { get; }

            /// <summary>
            /// トークテキスト置換設定を取得する。
            /// </summary>
            public TalkTextReplaceConfig TalkTextReplaceConfig { get; }

            /// <summary>
            /// AviUtl拡張編集ファイル用設定を取得する。
            /// </summary>
            public ExoConfig ExoConfig { get; }

            /// <summary>
            /// アプリ設定を取得する。
            /// </summary>
            public AppConfig AppConfig { get; }

            /// <summary>
            /// トークテキストを取得する。
            /// </summary>
            public string TalkText { get; }
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="processGetter">VOICEROIDプロセス取得デリゲート。</param>
        /// <param name="talkTextReplaceConfigGetter">
        /// トークテキスト置換設定取得デリゲート。
        /// </param>
        /// <param name="exoConfigGetter">
        /// AviUtl拡張編集ファイル用設定取得デリゲート。
        /// </param>
        /// <param name="appConfigGetter">アプリ設定取得デリゲート。</param>
        /// <param name="talkTextGetter">トークテキスト取得デリゲート。</param>
        /// <param name="resultNotifier">処理結果のアプリ状態通知デリゲート。</param>
        public SaveCommandExecuter(
            Func<IProcess> processGetter,
            Func<TalkTextReplaceConfig> talkTextReplaceConfigGetter,
            Func<ExoConfig> exoConfigGetter,
            Func<AppConfig> appConfigGetter,
            Func<string> talkTextGetter,
            Func<IAppStatus, Parameter, Task> resultNotifier)
            : base()
        {
            if (processGetter == null)
            {
                throw new ArgumentNullException(nameof(processGetter));
            }
            if (talkTextReplaceConfigGetter == null)
            {
                throw new ArgumentNullException(nameof(talkTextReplaceConfigGetter));
            }
            if (exoConfigGetter == null)
            {
                throw new ArgumentNullException(nameof(exoConfigGetter));
            }
            if (appConfigGetter == null)
            {
                throw new ArgumentNullException(nameof(appConfigGetter));
            }
            if (talkTextGetter == null)
            {
                throw new ArgumentNullException(nameof(talkTextGetter));
            }
            if (resultNotifier == null)
            {
                throw new ArgumentNullException(nameof(resultNotifier));
            }

            this.AsyncFunc = this.ExecuteAsync;
            this.ParameterConverter =
                _ =>
                    new Parameter(
                        processGetter(),
                        talkTextReplaceConfigGetter(),
                        exoConfigGetter(),
                        appConfigGetter(),
                        talkTextGetter());

            this.ResultNotifier = resultNotifier;
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="processGetter">VOICEROIDプロセス取得デリゲート。</param>
        /// <param name="talkTextReplaceConfigGetter">
        /// トークテキスト置換設定取得デリゲート。
        /// </param>
        /// <param name="exoConfigGetter">
        /// AviUtl拡張編集ファイル用設定取得デリゲート。
        /// </param>
        /// <param name="appConfigGetter">アプリ設定取得デリゲート。</param>
        /// <param name="talkTextGetter">トークテキスト取得デリゲート。</param>
        /// <param name="resultNotifier">処理結果のアプリ状態通知デリゲート。</param>
        public SaveCommandExecuter(
            Func<IProcess> processGetter,
            Func<TalkTextReplaceConfig> talkTextReplaceConfigGetter,
            Func<ExoConfig> exoConfigGetter,
            Func<AppConfig> appConfigGetter,
            Func<string> talkTextGetter,
            Action<IAppStatus, Parameter> resultNotifier)
            :
            this(
                processGetter,
                talkTextReplaceConfigGetter,
                exoConfigGetter,
                appConfigGetter,
                talkTextGetter,
                (resultNotifier == null) ?
                    (Func<IAppStatus, Parameter, Task>)null :
                    async (r, p) =>
                    {
                        resultNotifier(r, p);
                        await Task.FromResult(0);
                    })
        {
        }

        /// <summary>
        /// VOICEROID2の分割ファイル名にマッチする正規表現。
        /// </summary>
        private static readonly Regex RegexSplitFileName =
            new Regex(
                @"\-\d+\.(wav|txt)$",
                RegexOptions.IgnoreCase | RegexOptions.ECMAScript);

        /// <summary>
        /// WAVEファイルパスを作成する。
        /// </summary>
        /// <param name="config">アプリ設定。</param>
        /// <param name="charaName">キャラ名。</param>
        /// <param name="talkText">トークテキスト。</param>
        /// <param name="voiceroid2">VOICEROID2用ならば true 。</param>
        /// <returns>WAVEファイルパス。作成できないならば null 。</returns>
        private static async Task<string> MakeWaveFilePath(
            AppConfig config,
            string charaName,
            string talkText,
            bool voiceroid2)
        {
            if (
                config == null ||
                string.IsNullOrWhiteSpace(config.SaveDirectoryPath) ||
                charaName == null ||
                talkText == null)
            {
                return null;
            }

            var dirPath = config.SaveDirectoryPath;
            var baseName =
                FilePathUtil.MakeFileName(config.FileNameFormat, charaName, talkText);

            // 同名ファイルがあるならば名前の末尾に "[数字]" を付ける
            // 作成される可能性のあるテキストファイルや.exoファイルも確認する
            // VOICEROID2用の場合は連番ファイルも確認する
            var name = baseName;
            await Task.Run(
                () =>
                {
                    for (int i = 1; ; ++i)
                    {
                        var path = Path.Combine(dirPath, name);
                        if (
                            !File.Exists(path + @".wav") &&
                            !File.Exists(path + @".txt") &&
                            !File.Exists(path + @".exo"))
                        {
                            // VOICEROID2なら連番ファイルも存在チェック
                            var splitFileFound =
                                voiceroid2 &&
                                Directory.EnumerateFiles(dirPath, name + @"-*.*")
                                    .Select(fp => Path.GetFileName(fp))
                                    .Any(fn => RegexSplitFileName.IsMatch(fn));
                            if (!splitFileFound)
                            {
                                break;
                            }
                        }

                        name = baseName + @"[" + i + @"]";
                    }
                });

            return Path.Combine(dirPath, name + @".wav");
        }

        /// <summary>
        /// テキストをファイルへ書き出す。
        /// </summary>
        /// <param name="filePath">テキストファイルパス。</param>
        /// <param name="text">書き出すテキスト。</param>
        /// <param name="utf8">
        /// UTF-8で書き出すならば true 。CP932で書き出すならば false 。
        /// </param>
        /// <returns>
        /// 書き出しタスク。
        /// 成功した場合は true を返す。そうでなければ false を返す。
        /// </returns>
        private static async Task<bool> WriteTextFile(
            string filePath,
            string text,
            bool utf8)
        {
            if (string.IsNullOrWhiteSpace(filePath) || text == null)
            {
                return false;
            }

            var encoding = utf8 ? (new UTF8Encoding(false)) : Encoding.GetEncoding(932);

            // VOICEROID側がテキストファイル書き出し中だと失敗するので複数回試行
            bool saved = false;
            for (int i = 0; !saved && i < 10; ++i)
            {
                try
                {
                    using (var writer = new StreamWriter(filePath, false, encoding))
                    {
                        await writer.WriteAsync(text);
                    }

                    saved = true;
                }
                catch (IOException)
                {
                    await Task.Delay(50);
                    continue;
                }
                catch
                {
                    break;
                }
            }

            return saved;
        }

        /// <summary>
        /// 文字列内にキーワードが含まれているVOICEROID識別IDを取得する。
        /// </summary>
        /// <param name="src">文字列。</param>
        /// <returns>VOICEROID識別ID。見つからなければ null 。</returns>
        /// <remarks>
        /// 複数のキーワードが見つかった場合はより前方にあるものが優先される。
        /// </remarks>
        private static VoiceroidId? FindKeywordContainedVoiceroidId(string src)
        {
            if (src == null)
            {
                return null;
            }

            // src の中で最も手前に含まれているキーワードを検索する関数
            int? minIndexOf(IEnumerable<string> keywords) =>
                keywords?
                    .Select(k => src.IndexOf(k))
                    .Where(i => i >= 0)
                    .Min(i => (int?)i);

            // 全VOICEROIDの中から最も手前にキーワードが含まれているものを検索
            return
                ((VoiceroidId[])Enum.GetValues(typeof(VoiceroidId)))
                    .Select(id => new { id, index = minIndexOf(id.GetInfo().Keywords) })
                    .Where(v => v.index != null)
                    .OrderBy(v => (int)v.index)
                    .FirstOrDefault()?
                    .id;
        }

        /// <summary>
        /// 処理結果のアプリ状態通知デリゲートを取得する。
        /// </summary>
        private Func<IAppStatus, Parameter, Task> ResultNotifier { get; }

        /// <summary>
        /// 非同期の実処理を行う。
        /// </summary>
        /// <param name="parameter">コマンドパラメータ。</param>
        private async Task ExecuteAsync(Parameter parameter)
        {
            var process = parameter?.Process;
            var talkTextReplaceConfig = parameter?.TalkTextReplaceConfig;
            var exoConfig = parameter?.ExoConfig;
            var appConfig = parameter?.AppConfig;

            if (process == null || exoConfig == null || appConfig == null)
            {
                await this.NotifyResult(
                    parameter,
                    AppStatusType.Fail,
                    @"ファイル保存を開始できませんでした。");
                return;
            }

            if (!process.IsRunning || process.IsSaving || process.IsDialogShowing)
            {
                await this.NotifyResult(
                    parameter,
                    AppStatusType.Fail,
                    @"ファイル保存を開始できませんでした。");
                return;
            }

            // VOICEROID2フラグ
            bool voiceroid2 = (process.Id == VoiceroidId.Voiceroid2);

            // 基テキスト、音声用テキスト作成
            string text, voiceText;
            if (appConfig.UseTargetText)
            {
                // 本体側のテキストを使う設定ならそちらから取得
                voiceText = text = await process.GetTalkText();
                if (text == null)
                {
                    await this.NotifyResult(
                        parameter,
                        AppStatusType.Fail,
                        @"本体側の文章を取得できませんでした。");
                    return;
                }
                if (!process.CanSaveBlankText && string.IsNullOrWhiteSpace(voiceText))
                {
                    await this.NotifyResult(
                        parameter,
                        AppStatusType.Fail,
                        @"本体側の文章が空白文です。",
                        subStatusText: @"空白文を音声保存することはできません。");
                    return;
                }
            }
            else
            {
                // 基テキスト取得
                text = parameter?.TalkText;
                if (text == null)
                {
                    await this.NotifyResult(
                        parameter,
                        AppStatusType.Fail,
                        @"ファイル保存を開始できませんでした。");
                    return;
                }

                // 音声用テキスト作成
                voiceText = talkTextReplaceConfig?.VoiceReplaceItems.Replace(text) ?? text;
                if (!process.CanSaveBlankText && string.IsNullOrWhiteSpace(voiceText))
                {
                    await this.NotifyResult(
                        parameter,
                        AppStatusType.Fail,
                        @"文章の音声用置換結果が空白になります。",
                        subStatusText: @"空白文を音声保存することはできません。");
                    return;
                }
            }

            // 字幕用テキスト作成
            var fileText =
                talkTextReplaceConfig?.TextFileReplaceItems.Replace(text) ?? text;

            // キャラクター名取得
            // VOICEROID2ならばボイスプリセット名を使う
            var charaName = voiceroid2 ? (await process.GetVoicePresetName()) : process.Name;

            // WAVEファイルパス決定
            string filePath = null;
            try
            {
                filePath = await MakeWaveFilePath(appConfig, charaName, text, voiceroid2);
            }
            catch (Exception ex)
            {
                ThreadTrace.WriteException(ex);
                await this.NotifyResult(
                    parameter,
                    AppStatusType.Fail,
                    @"ファイル名の決定に失敗しました。");
                return;
            }
            if (filePath == null)
            {
                await this.NotifyResult(
                    parameter,
                    AppStatusType.Fail,
                    @"ファイル保存を開始できませんでした。");
                return;
            }

            // パスが正常かチェック
            var pathStatus = FilePathUtil.CheckPathStatus(filePath, pathIsFile: true);
            if (pathStatus.StatusType != AppStatusType.None)
            {
                await this.ResultNotifier(pathStatus, parameter);
                return;
            }

            // トークテキスト設定
            if (!appConfig.UseTargetText && !(await process.SetTalkText(voiceText)))
            {
                // VOICEROID2の場合、本体の入力欄が読み取り専用になることがある。
                // 再生時と違い、メッセージを返すのみでリカバリはしない。

                await this.NotifyResult(
                    parameter,
                    AppStatusType.Fail,
                    @"文章の設定に失敗しました。",
                    AppStatusType.Information,
                    voiceroid2 ? @"一度再生を行ってみてください。" : null);
                return;
            }

            // WAVEファイル保存
            var result = await process.Save(filePath);
            if (!result.IsSucceeded)
            {
                await this.NotifyResult(
                    parameter,
                    AppStatusType.Fail,
                    result.Error,
                    subStatusText: result.ExtraMessage);
                return;
            }

            var requiredFilePath = filePath;
            filePath = result.FilePath;

            var statusText = Path.GetFileName(filePath) + @" を保存しました。";

            // VOICEROID2かつファイル名が異なる
            // → ファイル分割されているので以降の処理は行わない
            if (
                voiceroid2 &&
                !string.Equals(
                    Path.GetFileNameWithoutExtension(requiredFilePath),
                    Path.GetFileNameWithoutExtension(filePath),
                    StringComparison.InvariantCultureIgnoreCase))
            {
                await this.NotifyResult(
                    parameter,
                    AppStatusType.Success,
                    statusText,
                    AppStatusType.Warning,
                    @"ファイル分割時は音声ファイル保存のみ行います。");
                return;
            }

            // テキストファイル保存
            if (appConfig.IsTextFileForceMaking)
            {
                var txtPath = Path.ChangeExtension(filePath, @".txt");
                if (!(await WriteTextFile(txtPath, fileText, appConfig.IsTextFileUtf8)))
                {
                    await this.NotifyResult(
                        parameter,
                        AppStatusType.Success,
                        statusText,
                        AppStatusType.Fail,
                        @"テキストファイルを保存できませんでした。");
                    return;
                }
            }

            // 以降の処理の対象となるキャラ
            // VOICEROID2ならボイスプリセット名からキャラ選別
            var voiceroidId =
                (voiceroid2 ? FindKeywordContainedVoiceroidId(charaName) : null) ??
                process.Id;

            // .exo ファイル保存
            if (appConfig.IsExoFileMaking)
            {
                var exoPath = Path.ChangeExtension(filePath, @".exo");
                var ok =
                    await this.DoWriteExoFile(
                        exoPath,
                        exoConfig,
                        voiceroidId,
                        filePath,
                        fileText);
                if (!ok)
                {
                    await this.NotifyResult(
                        parameter,
                        AppStatusType.Success,
                        statusText,
                        AppStatusType.Fail,
                        @".exo ファイルを保存できませんでした。");
                    return;
                }
            }

            // ゆっくりMovieMaker処理
            var warnText = await DoOperateYmm(filePath, voiceroidId, charaName, appConfig);

            await this.NotifyResult(
                parameter,
                AppStatusType.Success,
                statusText,
                (warnText == null) ? AppStatusType.None : AppStatusType.Warning,
                warnText ?? @"保存先フォルダーを開く",
                (warnText == null) ?
                    new ProcessStartCommand(@"explorer.exe", $@"/select,""{filePath}""") :
                    null,
                (warnText == null) ? Path.GetDirectoryName(filePath) : null);
        }

        /// <summary>
        /// 設定を基にAviUtl拡張編集ファイル書き出しを行う。
        /// </summary>
        /// <param name="exoFilePath">AviUtl拡張編集ファイルパス。</param>
        /// <param name="exoConfig">AviUtl拡張編集ファイル用設定。</param>
        /// <param name="voiceroidId">VOICEROID識別ID。</param>
        /// <param name="waveFilePath">WAVEファイルパス。</param>
        /// <param name="text">テキスト。</param>
        /// <returns>成功したならば true 。そうでなければ false 。</returns>
        private async Task<bool> DoWriteExoFile(
            string exoFilePath,
            ExoConfig exoConfig,
            VoiceroidId voiceroidId,
            string waveFilePath,
            string text)
        {
            var common = exoConfig.Common;
            var charaStyle = exoConfig.CharaStyles[voiceroidId];

            // フレーム数算出
            int frameCount = 0;
            try
            {
                var waveTime =
                    await Task.Run(() => (new WaveFileInfo(waveFilePath)).TotalTime);
                var f =
                    (waveTime.Ticks * common.Fps) /
                    (charaStyle.PlaySpeed.Begin * (TimeSpan.TicksPerSecond / 100));
                frameCount = (int)decimal.Ceiling(f);
            }
            catch (Exception ex)
            {
                ThreadTrace.WriteException(ex);
                return false;
            }

            var exo =
                new ExEditObject
                {
                    Width = common.Width,
                    Height = common.Height,
                    Length = frameCount + common.ExtraFrames,
                };

            // decimal の小数部桁数を取得
            var scale = (decimal.GetBits(common.Fps)[3] & 0xFF0000) >> 16;

            exo.FpsScale = (int)Math.Pow(10, scale);
            exo.FpsBase = decimal.Floor(common.Fps * exo.FpsScale);

            // テキストレイヤー追加
            {
                var item =
                    new LayerItem
                    {
                        BeginFrame = 1,
                        EndFrame = exo.Length,
                        LayerId = 1,
                        GroupId = common.IsGrouping ? 1 : 0,
                        IsClipping = charaStyle.IsTextClipping
                    };

                var c = charaStyle.Text.Clone();
                ExoTextStyleTemplate.ClearUnused(c);
                c.Text = text;
                item.Components.Add(c);
                item.Components.Add(charaStyle.Render.Clone());

                exo.LayerItems.Add(item);
            }

            // 音声レイヤー追加
            {
                var item =
                    new LayerItem
                    {
                        BeginFrame = 1,
                        EndFrame = frameCount,
                        LayerId = 2,
                        GroupId = common.IsGrouping ? 1 : 0,
                        IsAudio = true,
                    };

                item.Components.Add(
                    new AudioFileComponent
                    {
                        PlaySpeed = charaStyle.PlaySpeed.Clone(),
                        FilePath = waveFilePath,
                    });
                item.Components.Add(charaStyle.Play.Clone());

                exo.LayerItems.Add(item);
            }

            // ファイル書き出し
            try
            {
                await ExoFileReaderWriter.WriteAsync(exoFilePath, exo);
            }
            catch (Exception ex)
            {
                ThreadTrace.WriteException(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 『ゆっくりMovieMaker』プロセス操作インスタンスを取得する。
        /// </summary>
        private static YmmProcess YmmProcess { get; } = new YmmProcess();

        /// <summary>
        /// 『ゆっくりMovieMaker』プロセス操作失敗時のリトライ回数。
        /// </summary>
        private const int YmmRetryCount = 8;

        /// <summary>
        /// 『ゆっくりMovieMaker』プロセス操作失敗時のリトライインターバル。
        /// </summary>
        private static readonly TimeSpan YmmRetryInterval = TimeSpan.FromMilliseconds(250);

        /// <summary>
        /// 設定を基に『ゆっくりMovieMaker』の操作を行う。
        /// </summary>
        /// <param name="filePath">WAVEファイルパス。</param>
        /// <param name="voiceroidId">VOICEROID識別ID。</param>
        /// <param name="voiceroid2CharaName">VOICEROID2の場合に用いるキャラ名。</param>
        /// <param name="config">アプリ設定。</param>
        /// <returns>警告文字列。問題ないならば null 。</returns>
        private async Task<string> DoOperateYmm(
            string filePath,
            VoiceroidId voiceroidId,
            string voiceroid2CharaName,
            AppConfig config)
        {
            if (!config.IsSavedFileToYmm)
            {
                return null;
            }

            // YMMキャラ名決定
            string charaName =
                (voiceroidId == VoiceroidId.Voiceroid2) ?
                    voiceroid2CharaName : config.YmmCharaRelations[voiceroidId].YmmCharaName;

            string warnText = null;

            for (int ri = 0; ri <= YmmRetryCount; ++ri)
            {
                // リトライ時処理
                if (ri > 0)
                {
                    YmmProcess.Reset();
                    await Task.Delay(YmmRetryInterval);
                }

                // 状態更新
                try
                {
                    await YmmProcess.Update();
                }
                catch (Exception ex)
                {
                    ThreadTrace.WriteException(ex);
                    return @"ゆっくりMovieMakerの起動状態確認に失敗しました。";
                }

                // そもそも起動していないなら何もしない
                if (!YmmProcess.IsRunning)
                {
                    return null;
                }

                // タイムラインウィンドウが見つからなければ即失敗
                if (!YmmProcess.IsTimelineWindowFound)
                {
                    return @"ゆっくりMovieMakerのタイムラインが見つかりません。";
                }

                // コントロール群が見つからなければリトライ
                if (!YmmProcess.IsTimelineElementFound)
                {
                    warnText = @"ゆっくりMovieMakerのタイムラインを操作できませんでした。";
                    continue;
                }

                // ファイルパス設定
                if (!(await YmmProcess.SetTimelineSpeechEditValue(filePath)))
                {
                    warnText = @"ゆっくりMovieMakerへのファイルパス設定に失敗しました。";
                    continue;
                }

                // キャラ選択
                // そもそもキャラ名が存在しない場合は何もしない
                if (
                    config.IsYmmCharaSelecting &&
                    !string.IsNullOrEmpty(charaName) &&
                    (await YmmProcess.SelectTimelineCharaComboBoxItem(charaName)) == false)
                {
                    warnText = @"ゆっくりMovieMakerのキャラ選択に失敗しました。";
                    continue;
                }

                // ボタン押下
                if (
                    config.IsYmmAddButtonClicking &&
                    !(await YmmProcess.ClickTimelineSpeechAddButton()))
                {
                    warnText = @"ゆっくりMovieMakerの追加ボタン押下に失敗しました。";
                    continue;
                }

#if DEBUG
                // デバッグ時にはリトライ回数を報告
                if (ri > 0)
                {
                    warnText = @"YMMリトライ回数 : " + ri;
                    ThreadDebug.WriteLine(warnText);
                    return warnText;
                }
#endif // DEBUG

                return null;
            }

            return warnText;
        }

        /// <summary>
        /// 処理結果のアプリ状態を通知する。
        /// </summary>
        /// <param name="parameter">コマンドパラメータ。</param>
        /// <param name="statusType">状態種別。</param>
        /// <param name="statusText">状態テキスト。</param>
        /// <param name="subStatusType">オプショナルなサブ状態種別。</param>
        /// <param name="subStatusText">オプショナルなサブ状態テキスト。</param>
        /// <param name="subStatusCommand">オプショナルなサブ状態コマンド。</param>
        /// <param name="subStatusCommandTip">
        /// オプショナルなサブ状態コマンドのチップテキスト。
        /// </param>
        private Task NotifyResult(
            Parameter parameter,
            AppStatusType statusType = AppStatusType.None,
            string statusText = "",
            AppStatusType subStatusType = AppStatusType.None,
            string subStatusText = "",
            ICommand subStatusCommand = null,
            string subStatusCommandTip = "")
            =>
            this.ResultNotifier(
                new AppStatus
                {
                    StatusType = statusType,
                    StatusText = statusText ?? "",
                    SubStatusType = subStatusType,
                    SubStatusText = subStatusText ?? "",
                    SubStatusCommand = subStatusCommand,
                    SubStatusCommandTip =
                        string.IsNullOrEmpty(subStatusCommandTip) ?
                            null : subStatusCommandTip,
                },
                parameter);
    }
}
