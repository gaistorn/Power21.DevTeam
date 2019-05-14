# Docker build for Spark and Anaconda
This Docker build based on [anaconda3](https://hub.docker.com/r/continuumio/anaconda3/) and [docker-spark](https://github.com/gettyimages/docker-spark).

Use it in a standalone cluster with the accompanying docker-compose.yml, or as a base for more complex recipes.

## What it Gives You
 - Anaconda (2019. 3. released) and Python 3.7
 - XGBoost
 - Apache Cassandra Driver (Python)
 - Apache Spark 2.4.3
 - [Spark-sklearn](https://github.com/databricks/spark-sklearn)
 - [GraphFrames](https://graphframes.github.io/index.html) 0.7.1-spark2.4.3-s_2.11
 - [Mongo-Spark-Connector](https://spark-packages.org/package/mongodb/mongo-spark) 2.4.0

## Docker example

To run SparkPi, run the image with Docker:

```
docker run --rm -it -p 4040:4040 gaistorn/condapark bin/run-example SparkPi 10
```

To start a Jupyter Notebook server with Anaconda from a Docker image:

```
docker run -i -t -p 8888:8888 gaistorn/condapark /bin/bash -c "/opt/conda/bin/conda install jupyter -y --quiet && mkdir /opt/notebooks && /opt/conda/bin/jupyter notebook --notebook-dir=/opt/notebooks --ip='*' --port=8888 --no-browser"

```

To able to use Graphframes with pyspark, make sure you set PYSPARK_SUBMIT_ARGS env with value:

```
--packages graphframes:graphframes:0.3.0-spark2.0-s_2.11 pyspark-shell
```

## License

MIT
