﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using RucheHome.Text;

namespace RucheHome.AviUtl.ExEdit
{
    /// <summary>
    /// 標準描画コンポーネントを表すクラス。
    /// </summary>
    [DataContract(Namespace = "")]
    public class RenderComponent : ComponentBase, ICloneable
    {
        #region アイテム名定数群

        /// <summary>
        /// X座標を保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfX = @"X";

        /// <summary>
        /// Y座標を保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfY = @"Y";

        /// <summary>
        /// Z座標を保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfZ = @"Z";

        /// <summary>
        /// 拡大率を保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfScale = @"拡大率";

        /// <summary>
        /// 透明度を保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfTransparency = @"透明度";

        /// <summary>
        /// 回転角度を保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfRotation = @"回転";

        /// <summary>
        /// 合成モードを保持する拡張編集オブジェクトファイルアイテムの名前。
        /// </summary>
        public const string ExoFileItemNameOfBlendMode = @"blend";

        #endregion

        /// <summary>
        /// コンポーネント名。
        /// </summary>
        public static readonly string ThisComponentName = @"標準描画";

        /// <summary>
        /// 拡張編集オブジェクトファイルのアイテムコレクションに
        /// コンポーネント名が含まれているか否かを取得する。
        /// </summary>
        /// <param name="items">アイテムコレクション。</param>
        /// <returns>含まれているならば true 。そうでなければ false 。</returns>
        public static bool HasComponentName(IniFileItemCollection items) =>
            HasComponentNameCore(items, ThisComponentName);

        /// <summary>
        /// 拡張編集オブジェクトファイルのアイテムコレクションから
        /// コンポーネントを作成する。
        /// </summary>
        /// <param name="items">アイテムコレクション。</param>
        /// <returns>コンポーネント。</returns>
        public static RenderComponent FromExoFileItems(IniFileItemCollection items) =>
            FromExoFileItemsCore(items, () => new RenderComponent());

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public RenderComponent() : base()
        {
            // イベントハンドラ追加のためにプロパティ経由で設定
            this.X = new MovableValue<CoordConst>();
            this.Y = new MovableValue<CoordConst>();
            this.Z = new MovableValue<CoordConst>();
            this.Scale = new MovableValue<ScaleConst>();
            this.Transparency = new MovableValue<TransparencyConst>();
            this.Rotation = new MovableValue<RotationConst>();
        }

        /// <summary>
        /// コピーコンストラクタ。
        /// </summary>
        /// <param name="src">コピー元。</param>
        public RenderComponent(RenderComponent src) : base()
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            src.CopyToCore(this);
        }

        /// <summary>
        /// コンポーネント名を取得する。
        /// </summary>
        public override string ComponentName => ThisComponentName;

        /// <summary>
        /// X座標を取得または設定する。
        /// </summary>
        [ExoFileItem(ExoFileItemNameOfX, Order = 1)]
        [DataMember]
        public MovableValue<CoordConst> X
        {
            get => this.x;
            set => this.SetCoordProperty(ref this.x, value);
        }
        private MovableValue<CoordConst> x = null;

        /// <summary>
        /// Y座標を取得または設定する。
        /// </summary>
        [ExoFileItem(ExoFileItemNameOfY, Order = 2)]
        [DataMember]
        public MovableValue<CoordConst> Y
        {
            get => this.y;
            set => this.SetCoordProperty(ref this.y, value);
        }
        private MovableValue<CoordConst> y = null;

        /// <summary>
        /// Z座標を取得または設定する。
        /// </summary>
        [ExoFileItem(ExoFileItemNameOfZ, Order = 3)]
        [DataMember]
        public MovableValue<CoordConst> Z
        {
            get => this.z;
            set => this.SetCoordProperty(ref this.z, value);
        }
        private MovableValue<CoordConst> z = null;

        /// <summary>
        /// 拡大率を取得または設定する。
        /// </summary>
        [ExoFileItem(ExoFileItemNameOfScale, Order = 4)]
        [DataMember]
        public MovableValue<ScaleConst> Scale
        {
            get => this.scale;
            set =>
                this.SetPropertyWithPropertyChangedChain(
                    ref this.scale,
                    value ?? new MovableValue<ScaleConst>());
        }
        private MovableValue<ScaleConst> scale = null;

        /// <summary>
        /// 透明度を取得または設定する。
        /// </summary>
        [ExoFileItem(ExoFileItemNameOfTransparency, Order = 5)]
        [DataMember]
        public MovableValue<TransparencyConst> Transparency
        {
            get => this.transparency;
            set =>
                this.SetPropertyWithPropertyChangedChain(
                    ref this.transparency,
                    value ?? new MovableValue<TransparencyConst>());
        }
        private MovableValue<TransparencyConst> transparency = null;

        /// <summary>
        /// 回転角度を取得または設定する。
        /// </summary>
        [ExoFileItem(ExoFileItemNameOfRotation, Order = 6)]
        [DataMember]
        public MovableValue<RotationConst> Rotation
        {
            get => this.rotation;
            set =>
                this.SetPropertyWithPropertyChangedChain(
                    ref this.rotation,
                    value ?? new MovableValue<RotationConst>());
        }
        private MovableValue<RotationConst> rotation = null;

        /// <summary>
        /// 合成モードを取得または設定する。
        /// </summary>
        [ExoFileItem(ExoFileItemNameOfBlendMode, Order = 7)]
        public BlendMode BlendMode
        {
            get => this.blendMode;
            set =>
                this.SetProperty(
                    ref this.blendMode,
                    Enum.IsDefined(value.GetType(), value) ? value : BlendMode.Normal);
        }
        private BlendMode blendMode = BlendMode.Normal;

        /// <summary>
        /// BlendMode プロパティのシリアライズ用ラッパプロパティ。
        /// </summary>
        [DataMember(Name = nameof(BlendMode))]
        [SuppressMessage("CodeQuality", "IDE0051")]
        private string BlendModeString
        {
            get => this.BlendMode.ToString();
            set =>
                this.BlendMode =
                    Enum.TryParse(value, out BlendMode mode) ? mode : BlendMode.Normal;
        }

        /// <summary>
        /// このコンポーネントのクローンを作成する。
        /// </summary>
        /// <returns>クローン。</returns>
        public RenderComponent Clone() => new RenderComponent(this);

        /// <summary>
        /// X, Y, Z で同期するプロパティ名と同期用関数のディクショナリ。
        /// </summary>
        private static readonly ReadOnlyDictionary<
            string,
            Action<MovableValue<CoordConst>, MovableValue<CoordConst>>>
        CoordSyncProperties =
            new ReadOnlyDictionary<
                string,
                Action<MovableValue<CoordConst>, MovableValue<CoordConst>>>(
                new Dictionary<
                    string,
                    Action<MovableValue<CoordConst>, MovableValue<CoordConst>>>
                {
                    {
                        nameof(IMovableValue.MoveMode),
                        (src, dest) => dest.MoveMode = src.MoveMode
                    },
                    {
                        nameof(IMovableValue.IsAccelerating),
                        (src, dest) => dest.IsAccelerating = src.IsAccelerating
                    },
                    {
                        nameof(IMovableValue.IsDecelerating),
                        (src, dest) => dest.IsDecelerating = src.IsDecelerating
                    },
                    {
                        nameof(IMovableValue.Interval),
                        (src, dest) => dest.Interval = src.Interval
                    },
                });

        /// <summary>
        /// X, Y, Z プロパティ値を設定する。
        /// </summary>
        /// <param name="field">設定先フィールド。</param>
        /// <param name="value">設定値。</param>
        /// <param name="propertyName">
        /// プロパティ名。 CallerMemberNameAttribute により自動設定される。
        /// </param>
        private void SetCoordProperty(
            ref MovableValue<CoordConst> field,
            MovableValue<CoordConst> value)
        {
            if (field == value)
            {
                return;
            }

            if (field != null)
            {
                field.PropertyChanged -= this.OnCoordPropertyChanged;
            }

            // プロパティ値設定の実処理
            this.SetPropertyWithPropertyChangedChain(
                ref field,
                value ?? new MovableValue<CoordConst>());

            field.PropertyChanged += this.OnCoordPropertyChanged;

            // 全プロパティが変更されたものとして処理
            foreach (var name in CoordSyncProperties.Keys)
            {
                this.OnCoordPropertyChanged(field, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        /// X, Y, Z のプロパティ変更時に呼び出される。
        /// </summary>
        /// <param name="sender">呼び出し元。</param>
        /// <param name="e">イベントデータ。</param>
        private void OnCoordPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // プロパティ名に対応する同期用関数を取得
            if (
                !(sender is MovableValue<CoordConst> src) ||
                !CoordSyncProperties.TryGetValue(e.PropertyName, out var syncFunc))
            {
                return;
            }

            // 同期処理
            foreach (var dest in new[] { this.X, this.Y, this.Z })
            {
                if (dest != null && dest != src)
                {
                    syncFunc(src, dest);
                }
            }
        }

        /// <summary>
        /// デシリアライズの直前に呼び出される。
        /// </summary>
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context) => this.ResetDataMembers();

        #region ICloneable の明示的実装

        /// <summary>
        /// このオブジェクトのクローンを作成する。
        /// </summary>
        /// <returns>クローン。</returns>
        object ICloneable.Clone() => this.Clone();

        #endregion

        #region MovableValue{TConstants} ジェネリッククラス用の定数情報構造体群

        /// <summary>
        /// 座標用の定数情報クラス。
        /// </summary>
        [SuppressMessage("Design", "CA1034")]
        [SuppressMessage("Performance", "CA1815")]
        public struct CoordConst : IMovableValueConstants
        {
            public int Digits => 1;
            public decimal DefaultValue => 0;
            public decimal MinValue => -99999.9m;
            public decimal MaxValue => 99999.9m;
            public decimal MinSliderValue => -2000;
            public decimal MaxSliderValue => 2000;
        }

        /// <summary>
        /// 拡大率用の定数情報クラス。
        /// </summary>
        [SuppressMessage("Design", "CA1034")]
        [SuppressMessage("Performance", "CA1815")]
        public struct ScaleConst : IMovableValueConstants
        {
            public int Digits => 2;
            public decimal DefaultValue => 100;
            public decimal MinValue => 0;
            public decimal MaxValue => 5000;
            public decimal MinSliderValue => 0;
            public decimal MaxSliderValue => 800;
        }

        /// <summary>
        /// 透明度用の定数情報クラス。
        /// </summary>
        [SuppressMessage("Design", "CA1034")]
        [SuppressMessage("Performance", "CA1815")]
        public struct TransparencyConst : IMovableValueConstants
        {
            public int Digits => 1;
            public decimal DefaultValue => 0;
            public decimal MinValue => 0;
            public decimal MaxValue => 100;
            public decimal MinSliderValue => 0;
            public decimal MaxSliderValue => 100;
        }

        /// <summary>
        /// 回転角度用の定数情報クラス。
        /// </summary>
        [SuppressMessage("Design", "CA1034")]
        [SuppressMessage("Performance", "CA1815")]
        public struct RotationConst : IMovableValueConstants
        {
            public int Digits => 2;
            public decimal DefaultValue => 0;
            public decimal MinValue => -3600;
            public decimal MaxValue => 3600;
            public decimal MinSliderValue => -360;
            public decimal MaxSliderValue => 360;
        }

        #endregion
    }
}
