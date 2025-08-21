#!/usr/bin/env python3
"""
Test script to demonstrate enhanced variance for next 7 days forecasting
"""

import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
from datetime import datetime, timedelta

def analyze_enhanced_variance():
    """Analyze the enhanced dataset to show improved variance for next 7 days forecasting"""
    
    print("=" * 70)
    print("ENHANCED VARIANCE FOR NEXT 7 DAYS FORECASTING ANALYSIS")
    print("=" * 70)
    
    # Load the enhanced dataset
    try:
        df = pd.read_csv('supply_chain_data.csv')
        print(f"✓ Dataset loaded successfully: {len(df)} records")
    except FileNotFoundError:
        print("❌ Dataset not found. Please run sccc2.py first.")
        return
    
    # Convert date column
    df['Date'] = pd.to_datetime(df['Date'])
    
    # Basic statistics
    print(f"\n📊 DATASET OVERVIEW:")
    print(f"   • Date range: {df['Date'].min().strftime('%Y-%m-%d')} to {df['Date'].max().strftime('%Y-%m-%d')}")
    print(f"   • Total products: {df['SKU'].nunique()}")
    print(f"   • Product categories: {df['Product type'].unique()}")
    
    # Analyze variance patterns by product type
    print(f"\n🔍 VARIANCE ANALYSIS BY PRODUCT TYPE:")
    print("-" * 60)
    
    for product_type in df['Product type'].unique():
        product_data = df[df['Product type'] == product_type]
        demand = product_data['Number of products sold']
        
        print(f"\n{product_type.upper()}:")
        print(f"   • Average demand: {demand.mean():.1f} units")
        print(f"   • Demand range: {demand.min():.0f} - {demand.max():.0f} units")
        print(f"   • Standard deviation: {demand.std():.1f} units")
        print(f"   • Coefficient of variation: {(demand.std() / demand.mean() * 100):.1f}%")
        print(f"   • Variance: {demand.var():.1f}")
    
    # Analyze short-term variance (next 7 days patterns)
    print(f"\n📅 SHORT-TERM VARIANCE ANALYSIS (Next 7 Days):")
    print("-" * 60)
    
    # Get the most recent data for next 7 days analysis
    latest_date = df['Date'].max()
    recent_data = df[df['Date'] >= latest_date - timedelta(days=30)]  # Last 30 days
    
    print(f"Analyzing last 30 days (ending {latest_date.strftime('%Y-%m-%d')}) for next 7 days patterns:")
    
    # Daily variance analysis
    daily_variance = recent_data.groupby('Date')['Number of products sold'].agg(['mean', 'std', 'var'])
    
    print(f"\nDaily Variance Statistics (Last 30 days):")
    print(f"   • Average daily variance: {daily_variance['var'].mean():.1f}")
    print(f"   • Maximum daily variance: {daily_variance['var'].max():.1f}")
    print(f"   • Minimum daily variance: {daily_variance['var'].min():.1f}")
    
    # Weekly variance analysis
    recent_data['Week'] = recent_data['Date'].dt.isocalendar().week
    weekly_variance = recent_data.groupby('Week')['Number of products sold'].agg(['mean', 'std', 'var'])
    
    print(f"\nWeekly Variance Statistics (Last 30 days):")
    print(f"   • Average weekly variance: {weekly_variance['var'].mean():.1f}")
    print(f"   • Maximum weekly variance: {weekly_variance['var'].max():.1f}")
    print(f"   • Minimum weekly variance: {weekly_variance['var'].min():.1f}")
    
    # Day-of-week variance analysis
    print(f"\n📊 DAY-OF-WEEK VARIANCE ANALYSIS:")
    print("-" * 40)
    
    day_variance = recent_data.groupby(recent_data['Date'].dt.dayofweek)['Number of products sold'].agg(['mean', 'std', 'var'])
    weekdays = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday']
    
    for i, (day, stats) in enumerate(day_variance.iterrows()):
        print(f"   • {weekdays[day]}: Mean={stats['mean']:.1f}, Std={stats['std']:.1f}, Var={stats['var']:.1f}")
    
    # Analyze SKU-specific variance patterns
    print(f"\n🏷️ SKU-SPECIFIC VARIANCE PATTERNS:")
    print("-" * 50)
    
    # Get top 5 SKUs by variance
    sku_variance = df.groupby('SKU')['Number of products sold'].agg(['mean', 'std', 'var']).sort_values('var', ascending=False)
    
    print(f"Top 5 SKUs by Variance (Most variable for next 7 days forecasting):")
    for i, (sku, stats) in enumerate(sku_variance.head(5).iterrows()):
        print(f"   {i+1}. {sku}: Mean={stats['mean']:.1f}, Std={stats['std']:.1f}, Var={stats['var']:.1f}")
    
    # Analyze trend visibility for next 7 days
    print(f"\n📈 TREND VISIBILITY FOR NEXT 7 DAYS:")
    print("-" * 50)
    
    # Calculate trend strength for each SKU in recent data
    trend_analysis = []
    for sku in df['SKU'].unique():
        sku_data = recent_data[recent_data['SKU'] == sku].sort_values('Date')
        if len(sku_data) >= 7:  # Need at least 7 days for trend analysis
            # Calculate trend over last 7 days
            last_7_days = sku_data.tail(7)
            if len(last_7_days) == 7:
                x = np.arange(7)
                y = last_7_days['Number of products sold'].values
                slope = np.polyfit(x, y, 1)[0]
                trend_strength = abs(slope)
                trend_analysis.append((sku, slope, trend_strength))
    
    # Sort by trend strength
    trend_analysis.sort(key=lambda x: x[2], reverse=True)
    
    print(f"Trend Strength in Last 7 Days (units per day):")
    for i, (sku, slope, strength) in enumerate(trend_analysis[:5]):
        direction = "↗" if slope > 0 else "↘"
        print(f"   {i+1}. {sku}: {direction} {abs(slope):.2f} units/day (Strength: {strength:.2f})")
    
    # Analyze variance patterns that make next 7 days forecasting easier
    print(f"\n🎯 VARIANCE PATTERNS FOR NEXT 7 DAYS FORECASTING:")
    print("-" * 60)
    
    # Check for high variance periods
    high_variance_threshold = daily_variance['var'].quantile(0.8)
    high_variance_days = daily_variance[daily_variance['var'] > high_variance_threshold]
    
    print(f"High Variance Days (Top 20% - Easier to forecast trends):")
    print(f"   • Variance threshold: {high_variance_threshold:.1f}")
    print(f"   • Number of high variance days: {len(high_variance_days)}")
    
    # Show some examples
    high_variance_examples = high_variance_days.head(10)
    print(f"\nTop 10 High Variance Days:")
    for date, stats in high_variance_examples.iterrows():
        print(f"   • {date.strftime('%Y-%m-%d')}: Mean={stats['mean']:.1f}, Variance={stats['var']:.1f}")
    
    # Summary of improvements
    print(f"\n🎯 ENHANCEMENT SUMMARY FOR NEXT 7 DAYS FORECASTING:")
    print("-" * 60)
    print(f"✓ Enhanced daily micro-trends with 0.25x amplitude")
    print(f"✓ Pronounced weekly cycles with 0.20x amplitude")
    print(f"✓ Bi-weekly variations with 0.15x amplitude")
    print(f"✓ Day-specific patterns (Monday: +0.20x, Saturday: +0.30x)")
    print(f"✓ Month-end effects (+0.15x) and beginning effects (+0.12x)")
    print(f"✓ SKU-specific daily patterns for differentiation")
    print(f"✓ Enhanced trend variations with weekly/monthly cycles")
    print(f"✓ More frequent anomalies (4% vs 2%) for realistic variance")
    print(f"✓ Random walk component for natural variation")
    
    print(f"\n💡 Benefits for Next 7 Days Forecasting:")
    print(f"   • Clear daily patterns that are easy to detect")
    print(f"   • Obvious weekly cycles for trend identification")
    print(f"   • Distinct SKU behaviors for better differentiation")
    print(f"   • Enhanced variance makes trends more visible")
    print(f"   • Better Prophet seasonal detection")
    print(f"   • Improved XGBoost feature learning")

if __name__ == "__main__":
    analyze_enhanced_variance()
