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

# Define holidays and events (Indian context)
HOLIDAYS = {
    'Diwali': {'month': 11, 'boost': 2.5, 'duration': 5},
    'Holi': {'month': 3, 'boost': 1.8, 'duration': 3},
    'Eid': {'month': 4, 'boost': 1.6, 'duration': 3},
    'Dussehra': {'month': 10, 'boost': 1.4, 'duration': 3},
    'Christmas': {'month': 12, 'boost': 1.7, 'duration': 7},
    'New Year': {'month': 1, 'boost': 1.5, 'duration': 3},
    'Karva Chauth': {'month': 10, 'boost': 2.0, 'duration': 2},
    'Raksha Bandhan': {'month': 8, 'boost': 1.3, 'duration': 2}
}

# Define seasonal events
SEASONAL_EVENTS = {
    'Valentine\'s Day': {'month': 2, 'day': 14, 'boost': 1.8, 'duration': 7},
    'Mother\'s Day': {'month': 5, 'day': 8, 'boost': 1.6, 'duration': 5},
    'Women\'s Day': {'month': 3, 'day': 8, 'boost': 1.4, 'duration': 3},
    'Black Friday': {'month': 11, 'day': 25, 'boost': 3.0, 'duration': 4},
    'Cyber Monday': {'month': 11, 'day': 28, 'boost': 2.5, 'duration': 3},
    'Summer Sale': {'month': 6, 'boost': 1.5, 'duration': 30},
    'Winter Sale': {'month': 12, 'boost': 1.7, 'duration': 15}
}


def is_holiday_period(date, holiday_info):
    """Check if date falls within holiday period"""
    if holiday_info['month'] == date.month:
        # For holidays without specific day, assume mid-month
        if 'day' not in holiday_info:
            holiday_start = date.replace(day=15)
        else:
            holiday_start = date.replace(day=holiday_info['day'])

        holiday_end = holiday_start + timedelta(days=holiday_info['duration'])
        return holiday_start <= date <= holiday_end
    return False


def get_seasonal_multiplier(date, product_type):
    """Get seasonal multiplier based on product type and date"""
    month = date.month

    if product_type == 'skincare':
        # Higher demand in winter months
        if month in [11, 12, 1, 2]:
            return 1.3
        elif month in [6, 7, 8]:  # Monsoon season
            return 1.2
        else:
            return 1.0
    elif product_type == 'haircare':
        # Steady demand with slight summer increase
        if month in [4, 5, 6]:
            return 1.1
        else:
            return 1.0
    elif product_type == 'cosmetics':
        # Higher demand during festival seasons
        if month in [10, 11, 12, 2, 3]:
            return 1.2
        else:
            return 1.0

    return 1.0


def get_weekly_pattern(date):
    """Get weekly pattern multiplier"""
    weekday = date.weekday()  # 0=Monday, 6=Sunday

    # Higher sales on weekends and Fridays
    if weekday == 4:  # Friday
        return 1.2
    elif weekday in [5, 6]:  # Saturday, Sunday
        return 1.3
    else:
        return 1.0


def calculate_demand_with_events(base_demand, date, product_type):
    """Calculate demand considering all events and seasonality"""
    demand = base_demand

    # Apply seasonal multiplier
    seasonal_mult = get_seasonal_multiplier(date, product_type)
    demand *= seasonal_mult

    # Apply weekly pattern
    weekly_mult = get_weekly_pattern(date)
    demand *= weekly_mult

    # Check for holidays
    for holiday, info in HOLIDAYS.items():
        if is_holiday_period(date, info):
            demand *= info['boost']
            break

    # Check for seasonal events
    for event, info in SEASONAL_EVENTS.items():
        if is_holiday_period(date, info):
            demand *= info['boost']
            break

    return demand


# Generate data
data = []
start_date = datetime(2023, 1, 1)

for sku in range(1, NUM_SKUS + 1):
    product_type = random.choice(PRODUCT_TYPES)
    base_demand = random.uniform(200, 800)
    trend = random.uniform(-2, 3)  # Slightly positive trend
    seasonality = random.uniform(30, 100)
    period = random.uniform(25, 35)  # Monthly seasonality
    start_date_sku = start_date + timedelta(days=(sku - 1) * 10)

    for i in range(ENTRIES_PER_SKU):
        date = start_date_sku + timedelta(days=i)

        # Calculate base demand with seasonality
        seasonal_component = seasonality * math.sin(2 * math.pi * i / period)
        base_demand_with_trend = base_demand + trend * i + seasonal_component

        # Apply event-based multipliers
        demand = calculate_demand_with_events(base_demand_with_trend, date, product_type)

        # Add noise
        demand += random.gauss(0, 30)

        # Other variables
        price = round(random.uniform(10.0, 150.0), 2)
        availability = random.randint(70, 100)
        products_sold = max(0, int(demand))
        revenue = round(products_sold * price, 2)
        demographics = random.choice(DEMOGRAPHICS)
        stock_levels = random.randint(50, 200)
        lead_times = random.randint(1, 30)
        order_quantities = random.randint(10, 150)
        shipping_times = random.randint(1, 10)
        carrier = random.choice(CARRIERS)
        shipping_costs = round(random.uniform(5.0, 25.0), 2)
        supplier = random.choice(SUPPLIERS)
        location = random.choice(LOCATIONS)
        lead_time = random.randint(1, 30)
        production_volumes = random.randint(100, 1000)
        manufacturing_lead_time = random.randint(1, 30)
        manufacturing_costs = round(random.uniform(20.0, 120.0), 2)
        inspection = random.choice(INSPECTION_RESULTS)
        defect_rates = round(random.uniform(0.0, 5.0), 2)
        transport_mode = random.choice(TRANSPORT_MODES)
        route = random.choice(ROUTES)
        costs = round(random.uniform(100.0, 1000.0), 2)

        # Prophet-specific columns
        ds = date.strftime('%Y-%m-%d')  # Prophet expects 'ds' column
        y = products_sold  # Prophet expects 'y' column for target variable

        # Additional Prophet features
        is_weekend = 1 if date.weekday() >= 5 else 0
        day_of_week = date.weekday()
        month = date.month
        quarter = (date.month - 1) // 3 + 1
        year = date.year

        # Holiday indicators
        is_holiday = 0
        holiday_name = ''
        for holiday, info in HOLIDAYS.items():
            if is_holiday_period(date, info):
                is_holiday = 1
                holiday_name = holiday
                break

        # Event indicators
        is_event = 0
        event_name = ''
        for event, info in SEASONAL_EVENTS.items():
            if is_holiday_period(date, info):
                is_event = 1
                event_name = event
                break

        # Seasonality indicators
        seasonal_mult = get_seasonal_multiplier(date, product_type)

        data.append([
            ds, y, date.strftime('%Y-%m-%d'), product_type, f'SKU{sku}', price, availability,
            products_sold, revenue, demographics, stock_levels, lead_times,
            order_quantities, shipping_times, carrier, shipping_costs, supplier,
            location, lead_time, production_volumes, manufacturing_lead_time,
            manufacturing_costs, inspection, defect_rates, transport_mode, route, costs,
            is_weekend, day_of_week, month, quarter, year, is_holiday, holiday_name,
            is_event, event_name, seasonal_mult
        ])

# Write to CSV
with open(CSV_FILE_PATH, 'w', newline='', encoding='utf-8') as csvfile:
    writer = csv.writer(csvfile)
    writer.writerow([
        'ds', 'y', 'Date', 'Product type', 'SKU', 'Price', 'Availability',
        'Number of products sold', 'Revenue generated', 'Customer demographics',
        'Stock levels', 'Lead times', 'Order quantities', 'Shipping times',
        'Shipping carriers', 'Shipping costs', 'Supplier name', 'Location',
        'Lead time', 'Production volumes', 'Manufacturing lead time',
        'Manufacturing costs', 'Inspection results', 'Defect rates',
        'Transportation modes', 'Routes', 'Costs', 'is_weekend', 'day_of_week',
        'month', 'quarter', 'year', 'is_holiday', 'holiday_name', 'is_event',
        'event_name', 'seasonal_multiplier'
    ])
    for row in data:
        writer.writerow(row)

print(f'Enhanced CSV file with Prophet features generated at: {CSV_FILE_PATH}')
print(f'Total entries: {len(data)}')
print(
    f'Date range: {start_date.strftime("%Y-%m-%d")} to {(start_date + timedelta(days=ENTRIES_PER_SKU + (NUM_SKUS - 1) * 10)).strftime("%Y-%m-%d")}')

# Optional: Display sample of holidays and events included
print("\nHolidays and Events included:")
for holiday, info in HOLIDAYS.items():
    print(f"- {holiday}: Month {info['month']}, Boost {info['boost']}x, Duration {info['duration']} days")
for event, info in SEASONAL_EVENTS.items():
    print(f"- {event}: Month {info['month']}, Boost {info['boost']}x, Duration {info['duration']} days")