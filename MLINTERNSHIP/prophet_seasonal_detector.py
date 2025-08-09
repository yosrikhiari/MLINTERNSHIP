import sys
import json
import pandas as pd
import numpy as np
from prophet import Prophet
import warnings
warnings.filterwarnings('ignore')

def main():
    try:
        input_file_path = sys.argv[1]
        with open(input_file_path, 'r', encoding='utf-8') as f:
            input_data = json.load(f)
        
        historical_data = input_data['historical_data']
        horizon = input_data.get('horizon', 7)  # Get forecast horizon
        
        df = pd.DataFrame(historical_data)
        df['ds'] = pd.to_datetime(df['ds'])
        df = df.sort_values('ds').reset_index(drop=True)
        
        # Enhanced Prophet model with more sophisticated seasonality detection
        m = Prophet(
            yearly_seasonality=True,
            weekly_seasonality=True,
            daily_seasonality=False,
            seasonality_mode='multiplicative',
            changepoint_prior_scale=0.05,  # Reduce overfitting
            seasonality_prior_scale=10.0,   # Allow stronger seasonality
            holidays_prior_scale=10.0,      # Allow holiday effects
            interval_width=0.8              # Confidence intervals
        )
        
        # Add custom seasonalities for better detection
        m.add_seasonality(name='monthly', period=30.5, fourier_order=5)
        m.add_seasonality(name='quarterly', period=91.25, fourier_order=3)
        
        # Fit the model
        m.fit(df)
        
        # Create future dataframe for forecasting
        future = m.make_future_dataframe(periods=horizon)
        forecast = m.predict(future)
        
        # Extract detailed seasonal components for historical data
        historical_seasonal_spikes = {}
        historical_weekly_seasonality = {}
        historical_yearly_seasonality = {}
        historical_monthly_seasonality = {}
        historical_quarterly_seasonality = {}
        
        for i, row in df.iterrows():
            baseline = max(forecast.loc[i, 'trend'], 0.1)  # Prevent division by zero
            
            # Extract different seasonal components
            weekly_seasonal = forecast.loc[i, 'weekly'] if 'weekly' in forecast.columns else 0
            yearly_seasonal = forecast.loc[i, 'yearly'] if 'yearly' in forecast.columns else 0
            monthly_seasonal = forecast.loc[i, 'monthly'] if 'monthly' in forecast.columns else 0
            quarterly_seasonal = forecast.loc[i, 'quarterly'] if 'quarterly' in forecast.columns else 0
            
            # Calculate combined seasonal effect
            seasonal_effect = weekly_seasonal + yearly_seasonal + monthly_seasonal + quarterly_seasonal
            
            # Calculate spike multiplier with more sophisticated logic
            spike_multiplier = max(0.5, min(3.0, 1.0 + (seasonal_effect / baseline)))
            
            date_str = row['ds'].strftime('%Y-%m-%d')
            historical_seasonal_spikes[date_str] = float(spike_multiplier)
            historical_weekly_seasonality[date_str] = float(weekly_seasonal)
            historical_yearly_seasonality[date_str] = float(yearly_seasonal)
            historical_monthly_seasonality[date_str] = float(monthly_seasonal)
            historical_quarterly_seasonality[date_str] = float(quarterly_seasonal)
        
        # Extract seasonal multipliers for future dates
        future_seasonal_spikes = []
        future_weekly_seasonality = []
        future_yearly_seasonality = []
        future_monthly_seasonality = []
        future_quarterly_seasonality = []
        
        for i in range(len(df), len(forecast)):
            baseline = max(forecast.loc[i, 'trend'], 0.1)
            
            weekly_seasonal = forecast.loc[i, 'weekly'] if 'weekly' in forecast.columns else 0
            yearly_seasonal = forecast.loc[i, 'yearly'] if 'yearly' in forecast.columns else 0
            monthly_seasonal = forecast.loc[i, 'monthly'] if 'monthly' in forecast.columns else 0
            quarterly_seasonal = forecast.loc[i, 'quarterly'] if 'quarterly' in forecast.columns else 0
            
            seasonal_effect = weekly_seasonal + yearly_seasonal + monthly_seasonal + quarterly_seasonal
            spike_multiplier = max(0.5, min(3.0, 1.0 + (seasonal_effect / baseline)))
            
            future_seasonal_spikes.append(float(spike_multiplier))
            future_weekly_seasonality.append(float(weekly_seasonal))
            future_yearly_seasonality.append(float(yearly_seasonal))
            future_monthly_seasonality.append(float(monthly_seasonal))
            future_quarterly_seasonality.append(float(quarterly_seasonal))
        
        # Return comprehensive seasonal patterns
        result = {
            'historical_seasonal_spikes': historical_seasonal_spikes,
            'future_seasonal_spikes': future_seasonal_spikes,
            'historical_weekly_seasonality': historical_weekly_seasonality,
            'future_weekly_seasonality': future_weekly_seasonality,
            'historical_yearly_seasonality': historical_yearly_seasonality,
            'future_yearly_seasonality': future_yearly_seasonality,
            'historical_monthly_seasonality': historical_monthly_seasonality,
            'future_monthly_seasonality': future_monthly_seasonality,
            'historical_quarterly_seasonality': historical_quarterly_seasonality,
            'future_quarterly_seasonality': future_quarterly_seasonality,
            'seasonality_detected': True,
            'model_info': {
                'changepoint_prior_scale': 0.05,
                'seasonality_prior_scale': 10.0,
                'holidays_prior_scale': 10.0,
                'seasonality_mode': 'multiplicative'
            }
        }
        
        print(json.dumps(result))
        
    except Exception as e:
        print(f"Error: {str(e)}", file=sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    main()