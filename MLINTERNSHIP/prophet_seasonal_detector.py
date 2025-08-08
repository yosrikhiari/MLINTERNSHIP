import sys
import json
import pandas as pd
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
        
        # Enhanced Prophet model
        m = Prophet(
            yearly_seasonality=True,
            weekly_seasonality=True,
            daily_seasonality=False,
            seasonality_mode='multiplicative',
            changepoint_prior_scale=0.05,  # Reduce overfitting
            seasonality_prior_scale=10.0   # Allow stronger seasonality
        )
        
        m.fit(df)
        
        # Create future dataframe for forecasting
        future = m.make_future_dataframe(periods=horizon)
        forecast = m.predict(future)
        
        # Extract seasonal multipliers for historical data
        historical_seasonal_spikes = {}
        for i, row in df.iterrows():
            baseline = max(forecast.loc[i, 'trend'], 0.1)  # Prevent division by zero
            weekly_seasonal = forecast.loc[i, 'weekly'] if 'weekly' in forecast.columns else 0
            yearly_seasonal = forecast.loc[i, 'yearly'] if 'yearly' in forecast.columns else 0
            
            # Calculate spike multiplier
            seasonal_effect = weekly_seasonal + yearly_seasonal
            spike_multiplier = max(0.5, min(3.0, 1.0 + (seasonal_effect / baseline)))
            
            historical_seasonal_spikes[row['ds'].strftime('%Y-%m-%d')] = float(spike_multiplier)
        
        # Extract seasonal multipliers for future dates
        future_seasonal_spikes = []
        for i in range(len(df), len(forecast)):
            baseline = max(forecast.loc[i, 'trend'], 0.1)
            weekly_seasonal = forecast.loc[i, 'weekly'] if 'weekly' in forecast.columns else 0
            yearly_seasonal = forecast.loc[i, 'yearly'] if 'yearly' in forecast.columns else 0
            
            seasonal_effect = weekly_seasonal + yearly_seasonal
            spike_multiplier = max(0.5, min(3.0, 1.0 + (seasonal_effect / baseline)))
            
            future_seasonal_spikes.append(float(spike_multiplier))
        
        # Return both historical and future seasonal patterns
        result = {
            'historical_seasonal_spikes': historical_seasonal_spikes,
            'future_seasonal_spikes': future_seasonal_spikes
        }
        
        print(json.dumps(result))
        
    except Exception as e:
        print(f"Error: {str(e)}", file=sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    main()