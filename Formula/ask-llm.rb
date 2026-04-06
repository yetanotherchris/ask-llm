class AskLlm < Formula
  desc "Ask any large language model from your terminal via OpenAI-compatible APIs"
  homepage "https://github.com/yetanotherchris/ask-llm"
  version "1.5.0"
  license "MIT"

  on_macos do
    if Hardware::CPU.arm?
      url "https://github.com/yetanotherchris/ask-llm/releases/download/v1.5.0/askllm-v1.5.0-osx-arm64"
      sha256 "98bf0ec3f5358796368ab774059f49e03bc4e7e1e32e5783c3ab33818be0982c"
    else
      url "https://github.com/yetanotherchris/ask-llm/releases/download/v1.5.0/askllm-v1.5.0-osx-x64"
      sha256 "f88c31ec9e68979e63694c30414ac161a370c532a748fc6416113e6b8afe4211"
    end
  end

  on_linux do
    url "https://github.com/yetanotherchris/ask-llm/releases/download/v1.5.0/askllm-v1.5.0-linux-x64"
    sha256 "9b830ad5e0e9d1d5740a8b03705fd5f7a9802fd9c8023dfc154a8cb64d9187ec"
  end

  def install
    if OS.mac?
      if Hardware::CPU.arm?
        bin.install "askllm-v1.5.0-osx-arm64" => "askllm"
      else
        bin.install "askllm-v1.5.0-osx-x64" => "askllm"
      end
    else
      bin.install "askllm-v1.5.0-linux-x64" => "askllm"
    end
  end

  test do
    assert_match "USAGE:", shell_output("#{bin}/askllm --help")
  end
end





