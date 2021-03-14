import * as CloudWatch from 'aws-sdk/clients/cloudwatch';
import { createLogger } from '../utils/logger';

const logger = createLogger(`Metrics-${process.env.NODE_ENV}`);
const METRIC_NAMESPACE: string = 'FraggerZ';

export enum MetricNames {
    ERROR = 'Errors',
    THROTTLED = 'Throttles',
}

export enum UnitNames {
    COUNT = 'Count',
    MILLISECONDS = 'Milliseconds',
}

const client = new CloudWatch();
let metrics: CloudWatch.MetricData = [];

export function addMetric(metricName: string, dimensions: CloudWatch.Dimensions, value: number): void {
    metrics.push({
        Timestamp: new Date(),
        MetricName: metricName,
        Dimensions: dimensions,
        Value: value,
        Unit: UnitNames.COUNT,
    });
}

export async function flushMetrics(): Promise<void> {
    if(!metrics || metrics.length === 0) {
        return;
    }
    const metricData: CloudWatch.PutMetricDataInput = {
        MetricData: metrics,
        Namespace: METRIC_NAMESPACE,
    };
    try {
        await client.putMetricData(metricData).promise();
    } catch (err) {
        logger.error(`Failed to put metric data: ${err.message}`, {
            stack: err.stack,
        });
    } finally {
        metrics = [];
    }
}

export function addTimer(metricName: string, dimensions: CloudWatch.Dimensions, timeMS: number): void {
    metrics.push({
        Timestamp: new Date(),
        MetricName: metricName,
        Dimensions: dimensions,
        Value: timeMS,
        Unit: UnitNames.MILLISECONDS,
    });
}