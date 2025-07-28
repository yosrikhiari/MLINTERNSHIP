import csv
import random
import math
from datetime import datetime, timedelta
import pandas as pd

# Define constants
NUM_SKUS = 10  # Number of unique SKUs
ENTRIES_PER_SKU = 110  # Total entries per SKU
CSV_FILE_PATH = 'supply_chain_data.csv'

# Define categorical values
PRODUCT_TYPES = ['haircare', 'skincare', 'cosmetics']
DEMOGRAPHICS = ['Male', 'Female', 'Non-binary', 'Unknown']
CARRIERS = ['Carrier A', 'Carrier B', 'Carrier C']
SUPPLIERS = ['Supplier 1', 'Supplier 2', 'Supplier 3', 'Supplier 4', 'Supplier 5']
LOCATIONS = ['Mumbai', 'Delhi', 'Bangalore', 'Kolkata', 'Chennai']
INSPECTION_RESULTS = ['Pass', 'Fail', 'Pending']
TRANSPORT_MODES = ['Road', 'Rail', 'Air', 'Sea']
ROUTES = ['Route A', 'Route B', 'Route C']

# Define holidays with realistic durations (Indian context)
HOLIDAYS = {
    'Diwali': {'month': 11, 'day': 12, 'boost': 2.8, 'duration': 5, 'type': 'major'},
    'Holi': {'month': 3, 'day': 13, 'boost': 1.9, 'duration': 2, 'type': 'festival'},
    'Eid': {'month': 4, 'day': 21, 'boost': 1.7, 'duration': 2, 'type': 'festival'},
    'Dussehra': {'month': 10, 'day': 15, 'boost': 1.5, 'duration': 2, 'type': 'festival'},
    'Christmas': {'month': 12, 'day': 25, 'boost': 2.1, 'duration': 3, 'type': 'major'},
    'New Year': {'month': 1, 'day': 1, 'boost': 1.8, 'duration': 2, 'type': 'celebration'},
    'Karva Chauth': {'month': 10, 'day': 20, 'boost': 2.2, 'duration': 1, 'type': 'beauty'},
    'Raksha Bandhan': {'month': 8, 'day': 19, 'boost': 1.4, 'duration': 1, 'type': 'festival'}
}

# Define seasonal events with specific durations
SEASONAL_EVENTS = {
    'Valentine\'s Day': {'month': 2, 'day': 14, 'boost': 2.0, 'duration': 1, 'type': 'beauty'},
    'Mother\'s Day': {'month': 5, 'day': 8, 'boost': 1.8, 'duration': 1, 'type': 'gift'},
    'Women\'s Day': {'month': 3, 'day': 8, 'boost': 1.6, 'duration': 1, 'type': 'beauty'},
    'Black Friday': {'month': 11, 'day': 24, 'boost': 3.5, 'duration': 1, 'type': 'sale'},
    'Cyber Monday': {'month': 11, 'day': 27, 'boost': 2.8, 'duration': 1, 'type': 'sale'},
    'Summer Sale': {'month': 6, 'day': 15, 'boost': 1.6, 'duration': 7, 'type': 'sale'},
    'Winter Sale': {'month': 12, 'day': 26, 'boost': 2.0, 'duration': 5, 'type': 'sale'},
    'Back to School': {'month': 6, 'day': 1, 'boost': 1.3, 'duration': 15, 'type': 'seasonal'}
}

# Product type specific event multipliers
PRODUCT_EVENT_MULTIPLIERS = {
    'cosmetics': {
        'beauty': 1.5,  # Extra boost for beauty-related events
        'gift': 1.3,
        'festival': 1.4,
        'sale': 1.2,
        'major': 1.3,
        'celebration': 1.2,
        'seasonal': 1.0
    },
    'skincare': {
        'beauty': 1.4,
        'gift': 1.2,
        'festival': 1.1,
        'sale': 1.3,
        'major': 1.2,
        'celebration': 1.1,
        'seasonal': 1.2
    },
    'haircare': {
        'beauty': 1.2,
        'gift': 1.1,
        'festival': 1.2,
        'sale': 1.1,
        'major': 1.1,
        'celebration': 1.0,
        'seasonal': 1.1
    }
}


def is_event_period(date, event_info):
    """Check if date falls within event period with exact date matching"""
    if event_info['month'] != date.month:
        return False
    
    event_start = date.replace(day=event_info['day'])
    event_end = event_start + timedelta(days=event_info['duration'] - 1)
    
    return event_start <= date <= event_end


def get_seasonal_multiplier(date, product_type):
    """Get seasonal multiplier based on product type and date"""
    month = date.month

    if product_type == 'skincare':
        # Higher demand in winter months (dry skin)
        if month in [11, 12, 1, 2]:
            return 1.4
        elif month in [6, 7, 8]:  # Monsoon season (humidity issues)
            return 1.2
        elif month in [3, 4, 5]:  # Pre-summer prep
            return 1.1
        else:
            return 1.0
    elif product_type == 'haircare':
        # Higher demand during monsoon and winter
        if month in [6, 7, 8]:  # Monsoon - hair problems
            return 1.3
        elif month in [11, 12, 1]:  # Winter - dry hair
            return 1.2
        elif month in [4, 5]:  # Pre-monsoon prep
            return 1.1
        else:
            return 1.0
    elif product_type == 'cosmetics':
        # Higher demand during festival and wedding seasons
        if month in [10, 11, 12]:  # Peak festival/wedding season
            return 1.3
        elif month in [2, 3]:  # Spring festivals and events
            return 1.2
        elif month in [8, 9]:  # Post-monsoon festivities
            return 1.1
        else:
            return 1.0

    return 1.0


def get_weekly_pattern(date):
    """Get weekly pattern multiplier with more realistic patterns"""
    weekday = date.weekday()  # 0=Monday, 6=Sunday

    # Shopping patterns: Higher on weekends, moderate on Friday
    if weekday == 4:  # Friday
        return 1.15
    elif weekday == 5:  # Saturday - peak shopping day
        return 1.35
    elif weekday == 6:  # Sunday
        return 1.25
    elif weekday == 0:  # Monday - lower sales
        return 0.85
    elif weekday == 1:  # Tuesday - lowest sales
        return 0.80
    else:  # Wednesday, Thursday
        return 0.95


def calculate_demand_with_events(base_demand, date, product_type):
    """Calculate demand considering all events and seasonality with improved logic"""
    demand = base_demand

    # Apply seasonal multiplier first
    seasonal_mult = get_seasonal_multiplier(date, product_type)
    demand *= seasonal_mult

    # Apply weekly pattern
    weekly_mult = get_weekly_pattern(date)
    demand *= weekly_mult

    # Track applied events to avoid double counting
    event_applied = False
    applied_event_type = None

    # Check for holidays (priority over seasonal events)
    for holiday, info in HOLIDAYS.items():
        if is_event_period(date, info):
            # Apply base event boost
            event_boost = info['boost']
            
            # Apply product-specific multiplier
            product_mult = PRODUCT_EVENT_MULTIPLIERS[product_type].get(info['type'], 1.0)
            final_boost = event_boost * product_mult
            
            demand *= final_boost
            event_applied = True
            applied_event_type = holiday
            break

    # Check for seasonal events only if no holiday was applied
    if not event_applied:
        for event, info in SEASONAL_EVENTS.items():
            if is_event_period(date, info):
                # Apply base event boost
                event_boost = info['boost']
                
                # Apply product-specific multiplier
                product_mult = PRODUCT_EVENT_MULTIPLIERS[product_type].get(info['type'], 1.0)
                final_boost = event_boost * product_mult
                
                demand *= final_boost
                event_applied = True
                applied_event_type = event
                break

    return demand, event_applied, applied_event_type


def calculate_realistic_sales_metrics(demand, price, availability, stock_levels):
    """Calculate realistic sales metrics based on demand, price, and constraints"""
    # Price elasticity effect
    base_price = 50.0  # Assumed base price
    price_elasticity = -0.8  # Negative elasticity
    price_effect = (price / base_price) ** price_elasticity
    
    adjusted_demand = demand * price_effect
    
    # Availability constraint
    availability_factor = availability / 100.0
    constrained_demand = adjusted_demand * availability_factor
    
    # Stock constraint
    max_sellable = min(constrained_demand, stock_levels * 0.8)  # Don't sell all stock
    
    # Add some randomness for realism
    products_sold = max(0, int(max_sellable + random.gauss(0, max_sellable * 0.1)))
    
    # Revenue calculation
    revenue = products_sold * price
    
    return products_sold, revenue


# Generate data
data = []
start_date = datetime(2023, 1, 1)

for sku in range(1, NUM_SKUS + 1):
    product_type = random.choice(PRODUCT_TYPES)
    
    # More realistic base demand ranges by product type
    if product_type == 'cosmetics':
        base_demand = random.uniform(150, 600)
    elif product_type == 'skincare':
        base_demand = random.uniform(200, 500)
    else:  # haircare
        base_demand = random.uniform(100, 400)
    
    trend = random.uniform(-1, 2)  # More controlled trend
    seasonality = random.uniform(20, 60)
    period = 30.44  # Monthly seasonality (average days per month)
    start_date_sku = start_date + timedelta(days=(sku - 1) * 5)  # Stagger starts

    for i in range(ENTRIES_PER_SKU):
        date = start_date_sku + timedelta(days=i)

        # Calculate base demand with seasonality
        seasonal_component = seasonality * math.sin(2 * math.pi * i / period)
        base_demand_with_trend = base_demand + trend * i + seasonal_component

        # Apply event-based multipliers
        demand, event_applied, applied_event = calculate_demand_with_events(
            base_demand_with_trend, date, product_type
        )

        # Add controlled noise
        demand += random.gauss(0, base_demand * 0.05)  # 5% noise relative to base

        # Generate other realistic variables
        price = round(random.uniform(15.0, 120.0), 2)
        availability = random.randint(85, 100)  # Higher availability range
        stock_levels = random.randint(100, 300)  # Higher stock levels

        # Calculate realistic sales metrics
        products_sold, revenue = calculate_realistic_sales_metrics(
            demand, price, availability, stock_levels
        )

        # Other variables with improved logic
        demographics = random.choice(DEMOGRAPHICS)
        # FIXED: Generate separate lead times for different contexts
        procurement_lead_time = random.randint(3, 25)  # Time to procure materials
        order_quantities = max(50, int(demand * random.uniform(0.8, 1.5)))
        shipping_times = random.randint(2, 8)
        carrier = random.choice(CARRIERS)
        shipping_costs = round(random.uniform(8.0, 30.0), 2)
        supplier = random.choice(SUPPLIERS)
        location = random.choice(LOCATIONS)
        production_volumes = max(products_sold * 2, random.randint(200, 800))
        manufacturing_lead_time = random.randint(5, 25)  # Time to manufacture
        manufacturing_costs = round(price * random.uniform(0.3, 0.7), 2)
        inspection = random.choices(INSPECTION_RESULTS, weights=[85, 10, 5])[0]
        defect_rates = round(random.uniform(0.5, 3.5), 2)
        transport_mode = random.choice(TRANSPORT_MODES)
        route = random.choice(ROUTES)
        costs = round(revenue * random.uniform(0.6, 0.8), 2)

        # Additional features
        is_weekend = 1 if date.weekday() >= 5 else 0
        day_of_week = date.weekday()
        month = date.month
        quarter = (date.month - 1) // 3 + 1
        year = date.year

        # Event indicators
        is_holiday = 0
        holiday_name = ''
        is_event = 0
        event_name = ''
        
        if applied_event:
            if applied_event in HOLIDAYS:
                is_holiday = 1
                holiday_name = applied_event
            elif applied_event in SEASONAL_EVENTS:
                is_event = 1
                event_name = applied_event

        # Seasonality indicators
        seasonal_mult = get_seasonal_multiplier(date, product_type)

        # FIXED: Updated data row - removed duplicate lead time, kept distinct ones
        data.append([
            date.strftime('%Y-%m-%d'), product_type, f'SKU{sku}', price, availability,
            products_sold, round(revenue, 2), demographics, stock_levels, procurement_lead_time,
            order_quantities, shipping_times, carrier, shipping_costs, supplier,
            location, production_volumes, manufacturing_lead_time,
            round(manufacturing_costs, 2), inspection, defect_rates, transport_mode, 
            route, round(costs, 2), is_weekend, day_of_week, month, quarter, year, 
            is_holiday, holiday_name, is_event, event_name, round(seasonal_mult, 3)
        ])

# FIXED: Write to CSV - updated header to reflect distinct lead time columns
with open(CSV_FILE_PATH, 'w', newline='', encoding='utf-8') as csvfile:
    writer = csv.writer(csvfile)
    writer.writerow([
        'Date', 'Product type', 'SKU', 'Price', 'Availability',
        'Number of products sold', 'Revenue generated', 'Customer demographics',
        'Stock levels', 'Procurement lead time', 'Order quantities', 'Shipping times',
        'Shipping carriers', 'Shipping costs', 'Supplier name', 'Location',
        'Production volumes', 'Manufacturing lead time',
        'Manufacturing costs', 'Inspection results', 'Defect rates',
        'Transportation modes', 'Routes', 'Costs', 'is_weekend', 'day_of_week',
        'month', 'quarter', 'year', 'is_holiday', 'holiday_name', 'is_event',
        'event_name', 'seasonal_multiplier'
    ])
    for row in data:
        writer.writerow(row)

print(f'Optimized CSV file generated at: {CSV_FILE_PATH}')
print(f'Total entries: {len(data)}')
print(f'Date range: {start_date.strftime("%Y-%m-%d")} to {(start_date + timedelta(days=ENTRIES_PER_SKU + (NUM_SKUS - 1) * 5)).strftime("%Y-%m-%d")}')

# Display event summary
print("\n=== EVENT SUMMARY ===")
print("\nSingle-day Events:")
single_day_events = {k: v for k, v in {**HOLIDAYS, **SEASONAL_EVENTS}.items() if v['duration'] == 1}
for event, info in single_day_events.items():
    print(f"- {event}: {info['month']}/{info['day']}, Boost: {info['boost']}x")

print("\nMulti-day Events:")
multi_day_events = {k: v for k, v in {**HOLIDAYS, **SEASONAL_EVENTS}.items() if v['duration'] > 1}
for event, info in multi_day_events.items():
    print(f"- {event}: {info['month']}/{info['day']}, Duration: {info['duration']} days, Boost: {info['boost']}x")

print("\nProduct-specific multipliers applied for event types:")
for product, multipliers in PRODUCT_EVENT_MULTIPLIERS.items():
    print(f"\n{product.capitalize()}:")
    for event_type, mult in multipliers.items():
        print(f"  - {event_type}: {mult}x additional multiplier")

print("\n=== FIXED REDUNDANCY ISSUES ===")
print("✓ Removed duplicate 'Lead time' column")
print("✓ Kept distinct lead time columns:")
print("  - 'Procurement lead time': Time to procure raw materials")
print("  - 'Manufacturing lead time': Time to manufacture products")