

podman-build:
	podman build -t rwt:latest .

podman-run:
	podman run -it --rm -v $(pwd)/Data:/Data:Z -p 25555:25555 rwt:latest
