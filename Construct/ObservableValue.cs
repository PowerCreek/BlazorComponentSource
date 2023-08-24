using System.Runtime.CompilerServices;

namespace ComponentPreview.Construct
{ 

    public class ObservableValue<T>
    {
        public ObservableValue() { }
        public ObservableValue(T value) => Value = value;

        public void OnChangeOccur([CallerMemberName] string? key = null, T? value = default)
        => ChangedEventHandler?.Invoke(key!, value);

        private T? BackingField;
        public T Value { get => BackingField; set
            {
                if (BackingField?.Equals(value) == true)
                    return;

                OnChangeOccur(value: BackingField = value);
            }
        }

        public event Action<string, T?>? ChangedEventHandler;
        public static implicit operator T?(ObservableValue<T> obserableValue) => obserableValue.Value;
        public static implicit operator string?(ObservableValue<T> obserableValue) => obserableValue?.Value?.ToString();

        public static ObservableValue<T> operator ++(ObservableValue<T> observableValue)
        {
            if (observableValue.Value is null)
                throw new InvalidOperationException("Cannot increment a null value");

            object boxedValue = observableValue.Value;

            boxedValue = boxedValue switch
            {
                int intValue => ++intValue,
                long longValue => ++longValue,
                float floatValue => ++floatValue,
                double doubleValue => ++doubleValue,
                decimal decimalValue => ++decimalValue,
                _ => throw new InvalidOperationException($"The ++ operator is not defined for type {observableValue.Value.GetType().FullName}")
            };

            T? result = (T)boxedValue;
            observableValue.Value = result;
            return observableValue;
        }

        public static ObservableValue<T> operator --(ObservableValue<T> observableValue)
        {
            if (observableValue.Value is null)
                throw new InvalidOperationException("Cannot increment a null value");

            object boxedValue = observableValue.Value;

            boxedValue = boxedValue switch
            {
                int intValue => --intValue,
                long longValue => --longValue,
                float floatValue => --floatValue,
                double doubleValue => --doubleValue,
                decimal decimalValue => --decimalValue,
                _ => throw new InvalidOperationException($"The -- operator is not defined for type {observableValue.Value.GetType().FullName}")
            };

            T? result = (T)boxedValue;
            observableValue.Value = result;
            return observableValue;
        }

        public override bool Equals(object? obj)
        {
            return obj switch
            {
                ObservableValue<T> {Value: var o} => object.Equals(Value, o),
                T o => object.Equals(Value, o),
                _ => false
            };
        }

        public override string ToString()
        {
            return ((string)this)!;
        }
    }
}

