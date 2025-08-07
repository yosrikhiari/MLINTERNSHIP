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
        df = pd.DataFrame(historical_data)
        df['ds'] = pd.to_datetime(df['ds'])
        df = df.sort_values('ds').reset_index(drop=True)
        
        # Simple Prophet model for seasonal detection only
        m = Prophet(
            yearly_seasonality=True,
            weekly_seasonality=True,
            daily_seasonality=False,
            seasonality_mode='multiplicative'
        )
        
        m.fit(df)
        
        # Get seasonal components for existing dates
        forecast = m.predict(df)
        
        # Extract seasonal multiplier (combination of weekly + yearly seasonality)
        seasonal_spikes = {}
        for i, row in df.iterrows():
            # Use trend + seasonal components as spike indicator
            baseline = forecast.loc[i, 'trend']
            seasonal = forecast.loc[i, 'weekly'] + forecast.loc[i, 'yearly']
            spike_multiplier = max(0.5, min(3.0, 1.0 + (seasonal / baseline) if baseline > 0 else 1.0))
            seasonal_spikes[row['ds'].strftime('%Y-%m-%d')] = float(spike_multiplier)
        
        print(json.dumps(seasonal_spikes))
        
    except Exception as e:
        print(f"Error: {str(e)}", file=sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    main()