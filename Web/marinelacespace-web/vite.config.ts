import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig } from 'vite';

const apiGatewayUrl =
	process.env['services__api-gateway__https__0'] ||
	process.env['services__api-gateway__http__0'] ||
	'https://localhost:7200';

export default defineConfig({
	plugins: [sveltekit()],
	server: {
		port: 5173,
		proxy: {
			'/api': {
				target: apiGatewayUrl,
				changeOrigin: true,
				secure: false
			}
		}
	}
});
