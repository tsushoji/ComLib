using System.ComponentModel;

namespace ComApp.ViewModels
{
    public abstract class ViewModelBase: INotifyPropertyChanged
    {
        // プロパティ変更イベントハンドラー
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// プロパティをセット
        /// </summary>
        /// <typeparam name="T">フィールドの型</typeparam>
        /// <param name="field">フィールド</param>
        /// <param name="value">値</param>
        /// <param name="propertyName">プロパティ名</param>
        /// <returns>フィールドの値を更新した場合、true そうでない場合、false</returns>
        protected bool SetProperty<T>(
            ref T field, 
            T value, 
            string propertyName) {
            if (Equals(field, value)) {
                return false;
            }

            field = value;
            var handler = this.PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
            return true;
        }
    }
}
