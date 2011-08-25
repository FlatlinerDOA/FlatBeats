using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace FlatBeats.ViewModels
{
    using System.Collections.Generic;

    public class DualTileRowViewModel<T> : ViewModel where T : ViewModel
    {
        private T left;

        public T Left
        {
            get
            {
                return this.left;
            }
            set
            {
                if (this.left == value)
                {
                    return;
                }

                this.left = value;
                this.OnPropertyChanged("Left");
            }
        }

        private T right;

        public T Right
        {
            get
            {
                return this.right;
            }

            set
            {
                if (this.right == value)
                {
                    return;
                }

                this.right = value;
                this.OnPropertyChanged("Right");
            }
        }

        public static IEnumerable<DualTileRowViewModel<T>> Tile(IEnumerable<T> items)
        {
            bool isOdd = false;
            DualTileRowViewModel<T> row = new DualTileRowViewModel<T>();
            foreach (var item in items)
            {
                if (isOdd)
                {
                    row.Right = item;
                    yield return row;
                    row = new DualTileRowViewModel<T>();
                }
                else
                {
                    row.Left = item;
                }

                isOdd = !isOdd;
            }


            if (isOdd)
            {
                yield return row;
            }
        }
    }
}
