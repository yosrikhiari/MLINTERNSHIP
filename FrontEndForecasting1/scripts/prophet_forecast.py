import sys
import json
import pandas as pd
from prophet import Prophet
from datetime import datetime, timedelta
import warnings
import numpy as np

# Suppress warnings
warnings.filterwarnings('ignore')

# Define holidays and seasonal events with refined impact
HOLIDAYS = {
    'Diwali': {'month': 11, 'day': 15, 'duration': 5, 'prior_scale': 15.0},
    'Holi': {'month': 3, 'day': 15, 'duration': 3, 'prior_scale': 10.0},
    'Eid': {'month': 4, 'day': 15, 'duration': 3, 'prior_scale': 8.0},
    'Dussehra': {'month': 10, 'day': 15, 'duration': 3, 'prior_scale': 8.0},
    'Christmas': {'month': 12, 'day': 25, 'duration': 7, 'prior_scale': 12.0},
    'New Year': {'month': 1, 'day': 1, 'duration': 3, 'prior_scale': 10.0},
    'Karva Chauth': {'month': 10, 'day': 15, 'duration': 2, 'prior_scale': 12.0},
    'Raksha Bandhan': {'month': 8, 'day': 15, 'duration': 2, 'prior_scale': 8.0}
}

SEASONAL_EVENTS = {
    'Valentine\'s Day': {'month': 2, 'day': 14, 'duration': 7, 'prior_scale': 10.0},
    'Mother\'s Day': {'month': 5, 'day': 8, 'duration': 5, 'prior_scale': 8.0},
    'Women\'s Day': {'month': 3, 'day': 8, 'duration': 3, 'prior_scale': 6.0},
    'Black Friday': {'month': 11, 'day': 25, 'duration': 4, 'prior_scale': 20.0},
    'Cyber Monday': {'month': 11, 'day': 28, 'duration': 3, 'prior_scale': 15.0},
    'Summer Sale': {'month': 6, 'day': 15, 'duration': 30, 'prior_scale': 8.0},
    'Winter Sale': {'month': 12, 'day': 15, 'duration': 15, 'prior_scale': 10.0}
}


def create_holidays_df(start_year, end_year):
    """Create holidays dataframe with better error handling"""
    holidays = []
    for year in range(start_year, end_year + 1):
        for holiday, info in HOLIDAYS.items():
            try:
                ds = datetime(year, info['month'], info.get('day', 15))
                holidays.append({
                    'holiday': holiday,
                    'ds': ds,
                    'lower_window': 0,
                    'upper_window': info['duration'] - 1,
                    'prior_scale': info['prior_scale']
                })
            except ValueError:
                continue
        for event, info in SEASONAL_EVENTS.items():
            try:
                ds = datetime(year, info['month'], info['day'])
                holidays.append({
                    'holiday': event,
                    'ds': ds,
                    'lower_window': 0,
                    'upper_window': info['duration'] - 1,
                    'prior_scale': info['prior_scale']
                })
            except ValueError:
                continue
    return pd.DataFrame(holidays)


def get_seasonal_multiplier(date, product_type):
    """Enhanced seasonal multiplier with smoother transitions"""
    month = date.month

    if product_type == 'skincare':
        if month in [12, 1, 2]:
            return 1.4
        elif month in [11, 3]:
            return 1.2
        elif month in [6, 7, 8]:
            return 1.15
        else:
            return 1.0
    elif product_type == 'haircare':
        if month in [4, 5, 6]:
            return 1.15
        elif month in [3, 7]:
            return 1.05
        else:
            return 1.0
    elif product_type == 'cosmetics':
        if month in [10, 11, 12]:
            return 1.3
        elif month in [2, 3]:
            return 1.2
        else:
            return 1.0
    return 1.0


def validate_and_clean_data(df):
    """Enhanced data validation and cleaning"""
    original_length = len(df)

    df = df.drop_duplicates(subset=['ds'])
    df = df.dropna(subset=['y'])
    df = df[df['y'] >= 0]

    if len(df) > 20:
        q25 = df['y'].quantile(0.25)
        q75 = df['y'].quantile(0.75)
        iqr = q75 - q25

        if iqr > 0:
            lower_bound = q25 - 2.5 * iqr
            upper_bound = q75 + 2.5 * iqr
            outlier_mask = (df['y'] < lower_bound) | (df['y'] > upper_bound)
            outlier_count = outlier_mask.sum()

            if 0 < outlier_count < len(df) * 0.05:
                print(f"Removing {outlier_count} outliers", file=sys.stderr)
                df = df[~outlier_mask]

    df = df.sort_values('ds').reset_index(drop=True)
    print(f"Data cleaning: {original_length} -> {len(df)} records", file=sys.stderr)
    return df


def create_future_features(future_df):
    """Create future features for forecasting"""
    future_df['is_weekend'] = (future_df['ds'].dt.weekday >= 5).astype(int)
    return future_df


def post_process_forecasts(forecasts, historical_data, smoothing_alpha=0.3):
    """Enhanced post-processing of forecasts"""
    if not forecasts or len(forecasts) == 0:
        return forecasts

    # Fix type issue: ensure max comparison works with proper types
    forecasts = [max(0.0, float(f)) for f in forecasts]
    hist_mean = np.mean(historical_data['y'])
    hist_std = np.std(historical_data['y'])
    hist_max = np.max(historical_data['y'])

    reasonable_upper_bound = hist_mean + 3 * hist_std
    reasonable_upper_bound = min(reasonable_upper_bound, hist_max * 2)

    if len(forecasts) > 1:
        smoothed_forecasts = [forecasts[0]]
        if len(historical_data) >= 7:
            recent_trend = (historical_data['y'].iloc[-3:].mean() -
                            historical_data['y'].iloc[-7:-4].mean()) / 3
        else:
            recent_trend = 0

        for i in range(1, len(forecasts)):
            trend_adjustment = recent_trend * (i + 1) * 0.1
            smoothed_value = (smoothing_alpha * forecasts[i] +
                              (1 - smoothing_alpha) * smoothed_forecasts[i - 1] +
                              trend_adjustment)
            smoothed_value = max(0, min(smoothed_value, reasonable_upper_bound))
            smoothed_forecasts.append(smoothed_value)
        forecasts = smoothed_forecasts

    forecasts = [float(f) if not (np.isnan(f) or np.isinf(f)) else hist_mean for f in forecasts]
    return forecasts


def get_optimized_model_parameters(df, product_type):
    """Get optimized model parameters based on data analysis"""
    data_length = len(df)
    data_std = df['y'].std()
    data_mean = df['y'].mean()

    if data_length > 10:
        trend_strength = abs(df['y'].iloc[-5:].mean() - df['y'].iloc[:5].mean()) / data_mean
    else:
        trend_strength = 0.1

    params = {
        'interval_width': 0.80,
        'mcmc_samples': 0,
        'n_changepoints': min(25, max(5, data_length // 10)),
        'changepoint_range': 0.8,
        'yearly_seasonality': False,
        'weekly_seasonality': 5,
        'daily_seasonality': False,
        'seasonality_mode': 'additive'
    }

    if data_length < 30:
        params.update({
            'changepoint_prior_scale': 0.001,
            'seasonality_prior_scale': 0.1,
            'holidays_prior_scale': 0.1
        })
    elif data_length < 90:
        params.update({
            'changepoint_prior_scale': 0.01 if trend_strength < 0.2 else 0.05,
            'seasonality_prior_scale': 1.0,
            'holidays_prior_scale': 5.0
        })
    else:
        params.update({
            'changepoint_prior_scale': 0.05 if trend_strength < 0.3 else 0.1,
            'seasonality_prior_scale': 5.0,
            'holidays_prior_scale': 10.0
        })

    if product_type == 'cosmetics':
        params['holidays_prior_scale'] *= 1.5
    elif product_type == 'skincare':
        params['seasonality_prior_scale'] *= 1.3

    print(f"Model params: changepoint_scale={params['changepoint_prior_scale']}, "
          f"seasonality_scale={params['seasonality_prior_scale']}, "
          f"mode={params['seasonality_mode']}", file=sys.stderr)

    return params


def add_regressors_enhanced(model, df):
    """Add regressors with simplified selection"""
    added_regressors = []
    if 'is_weekend' in df.columns and df['is_weekend'].nunique() > 1:
        try:
            model.add_regressor('is_weekend', standardize=True, mode='additive')
            added_regressors.append('is_weekend')
        except Exception as e:
            print(f"Warning: Could not add regressor is_weekend: {str(e)}", file=sys.stderr)
    return added_regressors


def main():
    try:
        if len(sys.argv) < 2:
            raise ValueError("No input file path provided as argument")
        input_file_path = sys.argv[1]
        with open(input_file_path, 'r', encoding='utf-8') as f:
            input_data = json.load(f)

        historical_data = input_data['historical_data']
        product_type = input_data['product_type']
        required_horizon = input_data['required_horizon']

        if not historical_data:
            raise ValueError("No historical data provided")
        if required_horizon <= 0:
            raise ValueError(f"Invalid horizon: {required_horizon}")

        df = pd.DataFrame(historical_data)
        df['ds'] = pd.to_datetime(df['ds'])
        print(f"Initial data: {len(df)} records, product: {product_type}, horizon: {required_horizon}", file=sys.stderr)

        df = validate_and_clean_data(df)
        df = df.sort_values('ds').reset_index(drop=True)

        if len(df) < 10:
            raise ValueError(f"Insufficient data after cleaning: only {len(df)} records")

        start_year = df['ds'].dt.year.min()
        end_year = df['ds'].dt.year.max() + 2
        holidays_df = create_holidays_df(start_year, end_year)

        model_params = get_optimized_model_parameters(df, product_type)
        # Fix: Pass holidays to Prophet constructor, not as parameter
        m = Prophet(holidays=holidays_df, **{k: v for k, v in model_params.items() if k != 'holidays'})
        m.add_seasonality(name='monthly', period=30.5, fourier_order=5)

        added_regressors = add_regressors_enhanced(m, df)
        print(f"Added regressors: {added_regressors}", file=sys.stderr)

        try:
            m.fit(df)
            print("Model fitted successfully", file=sys.stderr)
        except Exception as e:
            print(f"Model fitting failed: {str(e)}", file=sys.stderr)
            m = Prophet(
                yearly_seasonality=False,
                weekly_seasonality=5,
                daily_seasonality=False,
                changepoint_prior_scale=0.001,
                seasonality_mode='additive',
                interval_width=0.80
            )
            m.fit(df[['ds', 'y']])
            added_regressors = []

        last_date = df['ds'].max()
        future_dates = [last_date + timedelta(days=i) for i in range(1, required_horizon + 1)]
        future_df = pd.DataFrame({'ds': future_dates})
        future_df = create_future_features(future_df)

        future_columns = ['ds'] + [reg for reg in added_regressors if reg in future_df.columns]
        future_df = future_df[future_columns]
        print(f"Future dataframe shape: {future_df.shape}, columns: {future_df.columns.tolist()}", file=sys.stderr)

        forecast = m.predict(future_df)
        raw_forecasts = forecast['yhat'].tolist()
        forecasts = post_process_forecasts(raw_forecasts, df, smoothing_alpha=0.3)

        if len(forecasts) != required_horizon:
            raise ValueError(f"Forecast length mismatch: expected {required_horizon}, got {len(forecasts)}")

        print(f"Generated {len(forecasts)} forecasts, range: [{min(forecasts):.2f}, {max(forecasts):.2f}]",
              file=sys.stderr)
        output = {'forecasts': forecasts}
        print(json.dumps(output), flush=True)

    except json.JSONDecodeError as e:
        print(f"JSON parsing error: {str(e)}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"Error: {str(e)}", file=sys.stderr)
        sys.exit(1)


if __name__ == "__main__":
    main()