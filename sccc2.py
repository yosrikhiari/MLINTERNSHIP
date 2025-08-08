import csv
import random
import math
from datetime import datetime, timedelta
from typing import Dict, List, Tuple

# Enhanced configuration for complex seasonality testing
NUM_SKUS = 10
ENTRIES_PER_SKU = 450
CSV_FILE_PATH = 'supply_chain_data.csv'

# Product categories with distinct seasonal behaviors
PRODUCT_CATEGORIES = {
    'skincare': {
        'base_demand': (150, 400),
        'winter_boost': 1.4,  # Dry skin season
        'monsoon_boost': 1.2,  # Humidity issues
        'summer_penalty': 0.9,
        'trend': (-0.2, 0.8)
    },
    'haircare': {
        'base_demand': (100, 350),
        'monsoon_boost': 1.5,  # Hair fall season
        'winter_boost': 1.1,
        'summer_boost': 1.2,  # Dandruff issues
        'trend': (-0.1, 1.0)
    },
    'cosmetics': {
        'base_demand': (200, 500),
        'festival_boost': 1.8,  # Wedding/festival season
        'valentine_boost': 1.6,
        'summer_penalty': 0.85,
        'trend': (0.0, 1.5)
    },
    'fragrances': {
        'base_demand': (80, 250),
        'winter_boost': 1.3,  # Longevity in cold weather
        'valentine_boost': 2.0,
        'festival_boost': 1.4,
        'trend': (-0.3, 1.2)
    },
    'suncare': {
        'base_demand': (50, 200),
        'summer_boost': 3.0,  # Peak summer demand
        'spring_boost': 1.5,
        'winter_penalty': 0.3,
        'trend': (0.2, 0.8)
    }
}

# Complex holiday and event calendar
MAJOR_HOLIDAYS = {
    'Diwali': {'start_month': 10, 'end_month': 11, 'peak_day': 15, 'boost': 2.8, 'duration': 14, 'buildup_days': 7},
    'Christmas': {'start_month': 12, 'end_month': 12, 'peak_day': 25, 'boost': 2.2, 'duration': 7, 'buildup_days': 10},
    'New Year': {'start_month': 12, 'end_month': 1, 'peak_day': 31, 'boost': 1.9, 'duration': 5, 'buildup_days': 3},
    'Holi': {'start_month': 3, 'end_month': 3, 'peak_day': 13, 'boost': 1.7, 'duration': 3, 'buildup_days': 2},
    'Eid': {'start_month': 4, 'end_month': 4, 'peak_day': 22, 'boost': 1.8, 'duration': 3, 'buildup_days': 5},
}

SEASONAL_EVENTS = {
    'Valentine\'s Day': {'month': 2, 'day': 14, 'boost': 2.1, 'duration': 7, 'buildup_days': 7},
    'Mother\'s Day': {'month': 5, 'day': 12, 'boost': 1.6, 'duration': 3, 'buildup_days': 3},
    'Black Friday': {'month': 11, 'day': 24, 'boost': 3.2, 'duration': 4, 'buildup_days': 3},
    'Cyber Monday': {'month': 11, 'day': 27, 'boost': 2.8, 'duration': 1, 'buildup_days': 0},
    'Summer Sale': {'month': 6, 'day': 15, 'boost': 1.8, 'duration': 15, 'buildup_days': 5},
    'Winter Sale': {'month': 1, 'day': 26, 'boost': 1.9, 'duration': 10, 'buildup_days': 3},
    'Back to School': {'month': 7, 'day': 1, 'boost': 1.4, 'duration': 20, 'buildup_days': 5},
}

# Wedding season patterns (India specific)
WEDDING_SEASONS = [
    {'start_month': 11, 'end_month': 2, 'intensity': 1.5},  # Peak wedding season
    {'start_month': 4, 'end_month': 5, 'intensity': 1.2},   # Spring weddings
]

# Monsoon patterns (affects different products differently)
MONSOON_PERIOD = {'start_month': 6, 'end_month': 9, 'intensity_curve': [0.8, 1.2, 1.4, 1.1]}

def get_complex_seasonality(date: datetime, product_type: str) -> float:
    """Generate complex seasonal patterns for Prophet to detect"""
    multiplier = 1.0
    month = date.month
    day_of_year = date.timetuple().tm_yday
    
    product_config = PRODUCT_CATEGORIES.get(product_type, PRODUCT_CATEGORIES['skincare'])
    
    # 1. Basic seasonal cycles (multiple harmonics for complexity)
    # Annual cycle with multiple harmonics
    annual_cycle = (
        0.3 * math.sin(2 * math.pi * day_of_year / 365.25) +
        0.2 * math.sin(4 * math.pi * day_of_year / 365.25) +
        0.1 * math.sin(6 * math.pi * day_of_year / 365.25)
    )
    multiplier += annual_cycle
    
    # 2. Product-specific seasonal adjustments
    if product_type == 'skincare':
        if month in [11, 12, 1, 2]:  # Winter - dry skin
            multiplier *= product_config.get('winter_boost', 1.0)
        elif month in [6, 7, 8, 9]:  # Monsoon - humidity issues
            multiplier *= product_config.get('monsoon_boost', 1.0)
        elif month in [4, 5]:  # Summer heat
            multiplier *= product_config.get('summer_penalty', 1.0)
            
    elif product_type == 'haircare':
        if month in [6, 7, 8, 9]:  # Monsoon - hair fall peak
            multiplier *= product_config.get('monsoon_boost', 1.0)
        elif month in [3, 4, 5]:  # Summer - dandruff issues
            multiplier *= product_config.get('summer_boost', 1.0)
            
    elif product_type == 'suncare':
        if month in [3, 4, 5, 6]:  # Summer peak
            multiplier *= product_config.get('summer_boost', 1.0)
        elif month in [11, 12, 1, 2]:  # Winter low
            multiplier *= product_config.get('winter_penalty', 1.0)
    
    # 3. Wedding season effects
    for wedding_season in WEDDING_SEASONS:
        if is_in_season_range(date, wedding_season['start_month'], wedding_season['end_month']):
            if product_type in ['cosmetics', 'fragrances']:
                multiplier *= wedding_season['intensity']
    
    # 4. Weekly patterns (Prophet should detect this)
    week_day = date.weekday()
    if week_day == 5:  # Saturday - shopping day
        multiplier *= 1.3
    elif week_day == 6:  # Sunday - family shopping
        multiplier *= 1.2
    elif week_day in [1, 2]:  # Tuesday, Wednesday - mid-week lull
        multiplier *= 0.9
    
    # 5. Month-end salary effect
    if date.day >= 28:
        multiplier *= 1.15
    elif date.day <= 3:  # Beginning of month
        multiplier *= 1.1
    elif 10 <= date.day <= 15:  # Mid-month lull
        multiplier *= 0.95
    
    return max(0.3, min(3.0, multiplier))

def calculate_event_boost(date: datetime, product_type: str) -> Tuple[float, bool, str]:
    """Calculate boost from holidays and events with buildup periods"""
    boost = 1.0
    event_active = False
    active_event = None
    
    # Check major holidays
    for holiday, info in MAJOR_HOLIDAYS.items():
        if is_in_event_period(date, info):
            # Create buildup effect
            days_to_peak = abs((date - get_peak_date(date.year, info)).days)
            if days_to_peak <= info['buildup_days']:
                buildup_factor = 1.0 - (days_to_peak / (info['buildup_days'] + 1)) * 0.3
            else:
                buildup_factor = 0.7  # Post-event decline
            
            event_boost = info['boost'] * buildup_factor
            
            # Product-specific holiday effects
            if holiday in ['Diwali', 'Christmas'] and product_type in ['cosmetics', 'fragrances']:
                event_boost *= 1.2
            elif holiday == 'Holi' and product_type == 'skincare':
                event_boost *= 0.8  # Less demand due to colors
            
            boost *= event_boost
            event_active = True
            active_event = holiday
            break
    
    # Check seasonal events
    if not event_active:
        for event, info in SEASONAL_EVENTS.items():
            if is_in_event_period_simple(date, info):
                days_to_event = abs((date - datetime(date.year, info['month'], info['day'])).days)
                if days_to_event <= info.get('buildup_days', 0):
                    buildup_factor = 1.0 - (days_to_event / (info.get('buildup_days', 1) + 1)) * 0.2
                else:
                    buildup_factor = 0.8
                
                event_boost = info['boost'] * buildup_factor
                
                # Product-specific event effects
                if event == 'Valentine\'s Day' and product_type in ['cosmetics', 'fragrances']:
                    event_boost *= 1.3
                elif event in ['Black Friday', 'Cyber Monday']:
                    event_boost *= 1.1  # Universal boost
                elif event == 'Summer Sale' and product_type == 'suncare':
                    event_boost *= 1.4
                
                boost *= event_boost
                event_active = True
                active_event = event
                break
    
    return boost, event_active, active_event or "None"

def is_in_season_range(date: datetime, start_month: int, end_month: int) -> bool:
    """Check if date falls within a seasonal range (handles year wrap)"""
    month = date.month
    if start_month <= end_month:
        return start_month <= month <= end_month
    else:  # Wraps around year (e.g., Nov to Feb)
        return month >= start_month or month <= end_month

def is_in_event_period(date: datetime, event_info: Dict) -> bool:
    """Check if date falls within event period with complex logic"""
    if 'start_month' in event_info and 'end_month' in event_info:
        return is_in_season_range(date, event_info['start_month'], event_info['end_month'])
    else:
        peak_date = get_peak_date(date.year, event_info)
        start_date = peak_date - timedelta(days=event_info.get('buildup_days', 0))
        end_date = peak_date + timedelta(days=event_info.get('duration', 1))
        return start_date <= date <= end_date

def is_in_event_period_simple(date: datetime, event_info: Dict) -> bool:
    """Simple event period check"""
    try:
        event_date = datetime(date.year, event_info['month'], event_info['day'])
        start_date = event_date - timedelta(days=event_info.get('buildup_days', 0))
        end_date = event_date + timedelta(days=event_info.get('duration', 1))
        return start_date <= date <= end_date
    except ValueError:
        return False

def get_peak_date(year: int, event_info: Dict) -> datetime:
    """Get peak date for an event"""
    return datetime(year, event_info.get('peak_month', event_info.get('start_month')), 
                   event_info.get('peak_day', 15))

def add_complex_noise(base_value: float, noise_level: float = 0.1) -> float:
    """Add complex noise patterns that Prophet should handle"""
    # Multiple noise components
    gaussian_noise = random.gauss(0, base_value * noise_level)
    
    # Occasional spikes (supply disruptions, viral social media, etc.)
    if random.random() < 0.02:  # 2% chance of anomaly
        spike = random.choice([0.3, 1.8, 2.5])  # Supply shortage or viral demand
        gaussian_noise += base_value * (spike - 1.0)
    
    # Weekly autocorrelation noise
    weekly_noise = base_value * 0.05 * math.sin(random.uniform(0, 2 * math.pi))
    
    return base_value + gaussian_noise + weekly_noise

def generate_enhanced_data():
    """Generate enhanced dataset with complex seasonality patterns"""
    data = []
    start_date = datetime(2023, 1, 1)  # Full year for better pattern detection
    
    print(f"Generating enhanced dataset with {NUM_SKUS} SKUs and {ENTRIES_PER_SKU} days each...")
    print("Complex patterns included:")
    print("- Multi-harmonic seasonal cycles")
    print("- Product-specific seasonality")
    print("- Holiday buildup and decay effects")
    print("- Wedding season patterns")
    print("- Weekly and monthly cycles")
    print("- Supply disruption anomalies")
    
    for sku in range(1, NUM_SKUS + 1):
        product_type = random.choice(list(PRODUCT_CATEGORIES.keys()))
        product_config = PRODUCT_CATEGORIES[product_type]
        
        # Initialize product-specific parameters
        base_demand = random.uniform(*product_config['base_demand'])
        trend = random.uniform(*product_config['trend'])
        
        # Multiple seasonal components for complexity
        annual_amplitude = random.uniform(30, 80)
        monthly_amplitude = random.uniform(15, 30)
        weekly_amplitude = random.uniform(10, 25)
        
        print(f"Generating SKU{sku} ({product_type}) - Base: {base_demand:.1f}, Trend: {trend:.3f}")
        
        for i in range(ENTRIES_PER_SKU):
            date = start_date + timedelta(days=i)
            
            # 1. Base trend
            base_with_trend = base_demand + (trend * i)
            
            # 2. Complex seasonality (multiple components)
            seasonal_multiplier = get_complex_seasonality(date, product_type)
            demand_with_seasonality = base_with_trend * seasonal_multiplier
            
            # 3. Event boosts
            event_boost, event_active, event_name = calculate_event_boost(date, product_type)
            demand_with_events = demand_with_seasonality * event_boost
            
            # 4. Add complex noise
            final_demand = add_complex_noise(demand_with_events, noise_level=0.08)
            final_demand = max(10, final_demand)  # Minimum realistic demand
            
            # Generate corresponding supply chain data
            price = round(random.uniform(15.0, 120.0), 2)
            availability = random.randint(85, 100)
            
            # Stock levels influenced by seasonality (smart inventory management)
            base_stock = random.randint(200, 500)
            if event_active:
                stock_multiplier = 1.3  # Stock up for events
            else:
                stock_multiplier = 1.0 + (seasonal_multiplier - 1.0) * 0.3  # Moderate stock adjustment
            
            stock_levels = int(base_stock * stock_multiplier)
            
            # Sales calculation (constrained by stock)
            max_possible_sales = min(int(final_demand * 1.2), stock_levels)
            products_sold = max(5, min(int(final_demand), max_possible_sales))
            
            revenue = round(products_sold * price, 2)
            
            # Supply chain metrics with some realism
            procurement_lead_time = random.randint(3, 25)
            manufacturing_lead_time = random.randint(5, 20)
            shipping_times = random.randint(1, 8)
            shipping_costs = round(random.uniform(8.0, 30.0), 2)
            manufacturing_costs = round(price * random.uniform(0.35, 0.65), 2)
            
            # Categorical variables
            demographics = random.choice(['Male', 'Female', 'Unisex'])
            carrier = random.choice(['Express', 'Standard', 'Premium', 'Economy'])
            supplier = random.choice([f'Supplier_{j}' for j in range(1, 8)])
            location = random.choice(['Mumbai', 'Delhi', 'Bangalore', 'Chennai', 'Kolkata', 'Pune'])
            
            # Date features
            is_weekend = 1 if date.weekday() >= 5 else 0
            day_of_week = date.weekday()
            month = date.month
            quarter = (date.month - 1) // 3 + 1
            year = date.year
            
            # Add derived features that might help both Prophet and XGBoost
            day_of_year = date.timetuple().tm_yday
            week_of_year = date.isocalendar()[1]
            is_month_end = 1 if date.day >= 28 else 0
            is_quarter_end = 1 if month % 3 == 0 and date.day >= 28 else 0
            
            data.append([
                date.strftime('%Y-%m-%d'),
                product_type,
                f'SKU{sku:03d}',
                price,
                availability,
                products_sold,
                revenue,
                demographics,
                stock_levels,
                procurement_lead_time,
                shipping_times,
                carrier,
                shipping_costs,
                supplier,
                location,
                manufacturing_lead_time,
                manufacturing_costs,
                is_weekend,
                day_of_week,
                month,
                quarter,
                year,
                day_of_year,
                week_of_year,
                is_month_end,
                is_quarter_end,
                1 if event_active else 0,
                event_name,
                round(seasonal_multiplier, 4)
            ])
    
    return data

def write_csv_data(data: List[List]) -> None:
    """Write data to CSV file"""
    headers = [
        'Date', 'Product type', 'SKU', 'Price', 'Availability',
        'Number of products sold', 'Revenue generated', 'Customer demographics',
        'Stock levels', 'Procurement lead time', 'Shipping times', 'Shipping carriers',
        'Shipping costs', 'Supplier name', 'Location', 'Manufacturing lead time',
        'Manufacturing costs', 'is_weekend', 'day_of_week', 'month', 'quarter', 'year',
        'day_of_year', 'week_of_year', 'is_month_end', 'is_quarter_end',
        'is_event_active', 'active_event', 'seasonal_multiplier'
    ]
    
    with open(CSV_FILE_PATH, 'w', newline='', encoding='utf-8') as csvfile:
        writer = csv.writer(csvfile)
        writer.writerow(headers)
        writer.writerows(data)
    
    print(f"\nEnhanced CSV generated: {CSV_FILE_PATH}")
    print(f"Total entries: {len(data):,}")
    print(f"Columns: {len(headers)}")
    print(f"Date range: {data[0][0]} to {data[-1][0]}")

def print_data_summary(data: List[List]) -> None:
    """Print summary statistics of generated data"""
    print("\n" + "="*50)
    print("DATA GENERATION SUMMARY")
    print("="*50)
    
    # Group by product type
    product_stats = {}
    event_count = 0
    
    for row in data:
        product_type = row[1]
        products_sold = row[5]
        is_event = row[27]
        
        if product_type not in product_stats:
            product_stats[product_type] = []
        product_stats[product_type].append(products_sold)
        
        if is_event:
            event_count += 1
    
    print(f"Total data points: {len(data):,}")
    print(f"Event periods: {event_count:,} ({event_count/len(data)*100:.1f}%)")
    print(f"Product categories: {len(product_stats)}")
    
    print("\nDemand by Product Type:")
    for product_type, sales in product_stats.items():
        avg_sales = sum(sales) / len(sales)
        max_sales = max(sales)
        min_sales = min(sales)
        print(f"  {product_type:12}: Avg={avg_sales:6.1f}, Range=[{min_sales:4.0f}, {max_sales:4.0f}]")
    
    print("\nComplex patterns included for Prophet detection:")
    print("✓ Multi-harmonic annual seasonality")
    print("✓ Product-specific seasonal behaviors")
    print("✓ Holiday buildup and decay effects")
    print("✓ Wedding season patterns (India-specific)")
    print("✓ Weekly shopping patterns")
    print("✓ Month-end salary effects")
    print("✓ Supply disruption anomalies")
    print("✓ Event-driven demand spikes")

if __name__ == "__main__":
    # Generate the enhanced dataset
    enhanced_data = generate_enhanced_data()
    
    # Write to CSV
    write_csv_data(enhanced_data)
    
    # Print summary
    print_data_summary(enhanced_data)
    
    print("\n" + "="*50)
    print("READY FOR PROPHET + XGBOOST TESTING!")
    print("="*50)
    print("This dataset contains complex seasonal patterns that will")
    print("challenge your Prophet seasonal detection and demonstrate")
    print("how well XGBoost can use Prophet's seasonal features.")